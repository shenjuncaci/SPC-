using Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Utilities;

namespace SPCSystem
{
    public class MRCalResultBll
    {
        Repository<CalFault> FaultRe = new Repository<CalFault>();
        /// <summary>
        /// 计算两条记录之间的MR值，后一条减去前一条
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public List<MRPictureDTO> GetMRList(List<MRPictureDTO> list)
        {
            for(int i=0;i<list.Count;i++)
            {
                if(i==0)
                {
                    list[i].mr = 0;
                }
                else
                {
                    list[i].mr = Math.Abs(list[i].x - list[i - 1].x);
                }
               
            }
            return list;
        }

        public List<MRPictureDTO> GetMrListReverse(List<MRPictureDTO> list)
        {
            for (int i = 0; i < list.Count-1; i++)
            {
                if (i == list.Count-1)
                {
                    list[i].mr = 0;
                }
                else
                {
                    list[i].mr = Math.Abs(list[i].x - list[i + 1].x);
                }

            }
            return list;
        }
        /// <summary>
        /// 0.正常无提示
        /// 1.一点落在A区控制限以外					
        /// 2.连续7点落在中心线的同一侧								
        /// 3.连续6点递增或递减					
        /// 4.连续14点中相邻点交替上下
        /// 5.连续3个点中有2个点落在中心线同一侧的B区以外
        /// 6.连续5个点中有4个点落在中心线同一侧的C区以外
        /// 7.连续15个点落在中心线两侧的c区以内
        /// 8.连续8个点落在中心线两侧且无一在C区内
        /// </summary>
        /// <param name="ProductCalID"></param>
        /// <returns></returns>
        public string CheckFault(string ProductCalID)
        {
            try
            {
                Repository<MRPictureDTO> re = new Repository<MRPictureDTO>();

                Repository<CalCriterion> reCriterion = new Repository<CalCriterion>();

                ///查询出需要进行检测的项目
                ///
                string sqlCheckNum = @"select * from [dbo].[CalCriterion] where 
ProductID in (select ProductID from CalMethod where CalID in (select CalID from ProductCal where ProductCalID='{0}'))";
                sqlCheckNum = string.Format(sqlCheckNum, ProductCalID);
                List<CalCriterion> listCheckNum = reCriterion.FindListBySql(sqlCheckNum);
                if (listCheckNum.Count() == 0)
                {
                    return "请先在基础数据中维护需要检测的项目。";
                }

                //
                bool check1 = false;
                bool check2 = false;
                bool check3 = false;
                bool check4 = false;
                bool check5 = false;
                bool check6 = false;
                bool check7 = false;
                bool check8 = false;
                if (listCheckNum.Find(x => x.CriterionNo == "001") != null)
                {
                    check1 = true;
                }
                if (listCheckNum.Find(x => x.CriterionNo == "002") != null)
                {
                    check2 = true;
                }
                if (listCheckNum.Find(x => x.CriterionNo == "003") != null)
                {
                    check3 = true;
                }
                if (listCheckNum.Find(x => x.CriterionNo == "004") != null)
                {
                    check4 = true;
                }
                if (listCheckNum.Find(x => x.CriterionNo == "005") != null)
                {
                    check5 = true;
                }
                if (listCheckNum.Find(x => x.CriterionNo == "006") != null)
                {
                    check6 = true;
                }
                if (listCheckNum.Find(x => x.CriterionNo == "007") != null)
                {
                    check7 = true;
                }
                if (listCheckNum.Find(x => x.CriterionNo == "008") != null)
                {
                    check8 = true;
                }

                string sql = @"select top 16 b.ProductCalDID,a.XCenterLine,a.XLowLimit,a.XUpperLimit,a.RCenterLine,a.RLowLimit,a.RUpperLimit,avg(c.InputValue) as x
From ProductCal a
left join ProductCalDetail b on a.ProductCalID=b.ProductCalID
left join ProductCalData c on b.ProductCalDID=c.ProductCalDID
where a.ProductCalID='{0}'
group by b.ProductCalDID,a.XCenterLine,a.XLowLimit,a.XUpperLimit,a.RCenterLine,a.RLowLimit,a.RUpperLimit,b.StartDate
order by b.StartDate desc";
                sql = string.Format(sql, ProductCalID);
                List<MRPictureDTO> list = re.FindListBySql(sql);
                list = GetMrListReverse(list);
                if(list.Count<=0)
                {
                    return "-9";
                }
                else if(list.Count<3)
                {
                    //case1
                    return Case1(list, ProductCalID,check1).ToString();
                }
                else if(list.Count>=3&&list.Count<5)
                {
                    //case1+case6
                    return Case1(list, ProductCalID, check1).ToString() + ":" + Case6(list, ProductCalID, check6).ToString();
                }
                else if (list.Count >= 5 && list.Count < 6)
                {
                    //判断case1+case5+case6
                    return Case1(list, ProductCalID, check1) + ":" + Case5(list, ProductCalID, check5) + ":" + Case6(list, ProductCalID, check6);
                }
                else if (list.Count >= 6 && list.Count < 7)
                {
                    //判断case1+case3+case5+case6
                    return Case1(list, ProductCalID, check1).ToString() + ":" + Case3(list, ProductCalID, check3).ToString() + ":" + Case5(list, ProductCalID, check5).ToString() + ":" + Case6(list, ProductCalID, check6);

                }
                else if (list.Count >= 7 && list.Count < 8)
                {
                    return Case1(list, ProductCalID, check1).ToString() + ":" + Case2(list, ProductCalID, check3).ToString() + ":" + Case3(list, ProductCalID, check2).ToString() + ":" + Case5(list, ProductCalID, check5) + ":" + Case6(list, ProductCalID, check6);
                }
                else if (list.Count >= 8 && list.Count < 14)
                {
                    //判断case1+case2+case3+5+6+8
                    return Case1(list, ProductCalID, check1) + ":" + Case2(list, ProductCalID, check2) + ":" + Case3(list, ProductCalID, check3) + ":" + Case5(list, ProductCalID, check5) + ":" + Case6(list, ProductCalID, check6) + ":" + Case8(list, ProductCalID, check8);
                }
                else if (list.Count == 14)
                {
                    // //判断case1+case2+case3+4+5+6+8
                    return Case1(list, ProductCalID, check1) + ":" + Case2(list, ProductCalID, check2) + ":" + Case3(list, ProductCalID, check3) + ":" + Case4(list, ProductCalID, check4) + ":" + Case5(list, ProductCalID, check5) + ":" + Case6(list, ProductCalID, check6) + ":" + Case8(list, ProductCalID, check8);
                }
                else
                {
                    //判断所有情况
                    return Case1(list, ProductCalID, check1) + ":" + Case2(list, ProductCalID, check2) + ":" + Case3(list, ProductCalID, check3) + ":" + Case4(list, ProductCalID, check4) + ":" + Case5(list, ProductCalID, check5) + ":" + Case6(list, ProductCalID, check6) + ":" + Case7(list, ProductCalID, check7) + ":" + Case8(list, ProductCalID, check8);
                }

            }
            catch
            {
                return "计算检验结果时出错了";
            }
        }

        public int Case1(List<MRPictureDTO> list, string ProductCalID, bool IsCheck)
        {
            if(IsCheck==false)
            {
                return 0;
            }
            int result = 0;
            if (list[0].x > list[0].XUpperLimit || list[0].x < list[0].XLowLimit)
            {
                //插入问题
                CalFault entity = new CalFault();
                entity.CalFaultID = CommonHelper.GetGuid;
                entity.ProductCalID = ProductCalID;
                entity.ProductCalDID = list[0].ProductCalDID;
                entity.FaultContent = "一点落在控制限以外";
                entity.FaultFrom = "X";
                entity.CreateDate = DateTime.Now;
                entity.CreatrBy = ManageProvider.Provider.Current().UserName;
                FaultRe.Insert(entity);
                result = -1;
            }
            //超过一个数据的话，判断MR数据
            if(list.Count>1)
            {
                if (list[0].mr > list[0].RUpperLimit || list[0].x < list[0].RLowLimit)
                {
                    //插入问题
                    CalFault entity = new CalFault();
                    entity.CalFaultID = CommonHelper.GetGuid;
                    entity.ProductCalID = ProductCalID;
                    entity.ProductCalDID = list[0].ProductCalDID;
                    entity.FaultContent = "一点落在控制限以外";
                    entity.FaultFrom = "MR";
                    entity.CreateDate = DateTime.Now;
                    entity.CreatrBy = ManageProvider.Provider.Current().UserName;
                    FaultRe.Insert(entity);
                    result = -1;
                }
            }
            return result;
        }
        /// <summary>
        /// 判断是否连续7点落在中心线的同一侧
        /// </summary>
        /// <param name="list"></param>
        /// <param name="ProductCalID"></param>
        /// <returns></returns>
        public int Case2(List<MRPictureDTO> list, string ProductCalID, bool IsCheck)
        {
            if (IsCheck == false)
            {
                return 0;
            }
            int result = 0;
            //1表示上侧-1表示下侧
            int AboveOrBelow = 0;
            if (list[0].x > list[0].XCenterLine)
            {
                AboveOrBelow = 1;
                for (int i = 1; i < 7; i++)
                {
                    if (list[i].x <= list[0].XCenterLine)
                    {
                        AboveOrBelow = 0;
                        break;
                    }
                    else
                    {
                        //AboveOrBelow = 1;

                    }
                }
                if (AboveOrBelow == 1)
                {
                    // 表示全部在中心线的上侧
                    CalFault entity = new CalFault();
                    entity.CalFaultID = CommonHelper.GetGuid;
                    entity.ProductCalID = ProductCalID;
                    entity.ProductCalDID = list[0].ProductCalDID;
                    entity.FaultContent = "连续7点落在中心线的上侧";
                    entity.FaultFrom = "X";
                    entity.CreateDate = DateTime.Now;
                    entity.CreatrBy = ManageProvider.Provider.Current().UserName;
                    FaultRe.Insert(entity);
                    result = -2;
                }
            }
            else if (list[0].x < list[0].XCenterLine)
            {
                AboveOrBelow = -1;
                for (int i = 1; i < 7; i++)
                {
                    if (list[i].x >= list[0].XCenterLine)
                    {
                        AboveOrBelow = 0;
                        break;
                    }
                    else
                    {

                    }
                }
                if (AboveOrBelow == -1)
                {
                    // 表示全部在中心线的上侧
                    CalFault entity = new CalFault();
                    entity.CalFaultID = CommonHelper.GetGuid;
                    entity.ProductCalID = ProductCalID;
                    entity.ProductCalDID = list[0].ProductCalDID;
                    entity.FaultContent = "连续7点落在中心线的下侧";
                    entity.FaultFrom = "X";
                    entity.CreateDate = DateTime.Now;
                    entity.CreatrBy = ManageProvider.Provider.Current().UserName;
                    FaultRe.Insert(entity);
                    result = -2;
                }
            }
            else
            {

            }
            if(list.Count>=8)
            {
                if (list[0].mr > list[0].RCenterLine)
                {
                    AboveOrBelow = 1;
                    for (int i = 1; i < 7; i++)
                    {
                        if (list[i].mr <= list[0].RCenterLine)
                        {
                            AboveOrBelow = 0;
                            break;
                        }
                        else
                        {
                            // 表示全部在中心线的上侧

                        }
                    }
                    if (AboveOrBelow == 1)
                    {
                        CalFault entity = new CalFault();
                        entity.CalFaultID = CommonHelper.GetGuid;
                        entity.ProductCalID = ProductCalID;
                        entity.ProductCalDID = list[0].ProductCalDID;
                        entity.FaultContent = "连续7点落在中心线的上侧";
                        entity.FaultFrom = "MR";
                        entity.CreateDate = DateTime.Now;
                        entity.CreatrBy = ManageProvider.Provider.Current().UserName;
                        FaultRe.Insert(entity);
                        result = -2;
                    }
                }
                else if (list[0].mr < list[0].RCenterLine)
                {
                    AboveOrBelow = -1;
                    for (int i = 1; i < 7; i++)
                    {
                        if (list[i].mr >= list[0].RCenterLine)
                        {
                            AboveOrBelow = 0;
                            break;
                        }
                        else
                        {
                            // 表示全部在中心线的上侧

                        }
                    }
                    if (AboveOrBelow == -1)
                    {
                        CalFault entity = new CalFault();
                        entity.CalFaultID = CommonHelper.GetGuid;
                        entity.ProductCalID = ProductCalID;
                        entity.ProductCalDID = list[0].ProductCalDID;
                        entity.FaultContent = "连续7点落在中心线的下侧";
                        entity.FaultFrom = "MR";
                        entity.CreateDate = DateTime.Now;
                        entity.CreatrBy = ManageProvider.Provider.Current().UserName;
                        FaultRe.Insert(entity);
                        result = -2;
                    }
                }
                else
                {

                }
            }

            return result;


        }
        /// <summary>
        /// 连续6点递增或递减
        /// </summary>
        /// <param name="list"></param>
        /// <param name="ProductCalID"></param>
        /// <returns></returns>
        public int Case3(List<MRPictureDTO> list, string ProductCalID, bool IsCheck)
        {
            if (IsCheck == false)
            {
                return 0;
            }
            int result = 0;
            if (list[0].x < list[1].x && list[1].x < list[2].x && list[2].x < list[3].x && list[3].x < list[4].x && list[4].x < list[5].x)
            {
                CalFault entity = new CalFault();
                entity.CalFaultID = CommonHelper.GetGuid;
                entity.ProductCalID = ProductCalID;
                entity.ProductCalDID = list[0].ProductCalDID;
                entity.FaultContent = "连续6点递减";
                entity.FaultFrom = "X";
                entity.CreateDate = DateTime.Now;
                entity.CreatrBy = ManageProvider.Provider.Current().UserName;
                FaultRe.Insert(entity);
                result = -3;
            }
            if (list[0].x > list[1].x && list[1].x > list[2].x && list[2].x > list[3].x && list[3].x > list[4].x && list[4].x > list[5].x)
            {
                CalFault entity = new CalFault();
                entity.CalFaultID = CommonHelper.GetGuid;
                entity.ProductCalID = ProductCalID;
                entity.ProductCalDID = list[0].ProductCalDID;
                entity.FaultContent = "连续6点递增";
                entity.FaultFrom = "X";
                entity.CreateDate = DateTime.Now;
                entity.CreatrBy = ManageProvider.Provider.Current().UserName;
                FaultRe.Insert(entity);
                result = -3;
            }
            if(list.Count>6)
            {
                if (list[0].mr < list[1].mr && list[1].mr < list[2].mr && list[2].mr < list[3].mr && list[3].mr < list[4].mr && list[4].mr < list[5].mr)
                {
                    CalFault entity = new CalFault();
                    entity.CalFaultID = CommonHelper.GetGuid;
                    entity.ProductCalID = ProductCalID;
                    entity.ProductCalDID = list[0].ProductCalDID;
                    entity.FaultContent = "连续6点递增";
                    entity.FaultFrom = "MR";
                    entity.CreateDate = DateTime.Now;
                    entity.CreatrBy = ManageProvider.Provider.Current().UserName;
                    FaultRe.Insert(entity);
                    result = -3;
                }
                if (list[0].mr > list[1].mr && list[1].mr > list[2].mr && list[2].mr > list[3].mr && list[3].mr > list[4].mr && list[4].mr > list[5].mr)
                {
                    CalFault entity = new CalFault();
                    entity.CalFaultID = CommonHelper.GetGuid;
                    entity.ProductCalID = ProductCalID;
                    entity.ProductCalDID = list[0].ProductCalDID;
                    entity.FaultContent = "连续6点递增";
                    entity.FaultFrom = "MR";
                    entity.CreateDate = DateTime.Now;
                    entity.CreatrBy = ManageProvider.Provider.Current().UserName;
                    FaultRe.Insert(entity);
                    result = -3;
                }
            }
            return result;
        }

        /// <summary>
        /// 判断连续14点是否交替上下
        /// </summary>
        /// <param name="list"></param>
        /// <param name="ProductCalID"></param>
        /// <returns></returns>
        public int Case4(List<MRPictureDTO> list, string ProductCalID, bool IsCheck)
        {
            if (IsCheck == false)
            {
                return 0;
            }
            int result = 0;
            //判断下一次判断是大于还是小于
            bool Next = true;
            if (list[0].x > list[1].x)
            {
                Next = false;
                result = -4;
            }
            else if (list[0].x < list[1].x)
            {
                Next = true;
                result = -4;
            }
            else
            {
                result = 0;
            }
            if (result == 0)
            { }
            else
            {
                for (int i = 1; i < 12; i++)
                {
                    if (Next == false)
                    {
                        if (list[i].x < list[i + 1].x)
                        {
                            Next = true;
                        }
                        else
                        {
                            result = 0;
                            break;
                        }
                    }
                    else
                    {
                        if (list[i].x > list[i + 1].x)
                        {
                            Next = false;
                        }
                        else
                        {
                            result = 0;
                            break;
                        }
                    }
                }
                if (result == -4)
                {
                    CalFault entity = new CalFault();
                    entity.CalFaultID = CommonHelper.GetGuid;
                    entity.ProductCalID = ProductCalID;
                    entity.ProductCalDID = list[0].ProductCalDID;
                    entity.FaultContent = "连续14点交替上下";
                    entity.FaultFrom = "X";
                    entity.CreateDate = DateTime.Now;
                    entity.CreatrBy = ManageProvider.Provider.Current().UserName;
                    FaultRe.Insert(entity);
                }

            }
            if(list.Count>14)
            {
                //判断R
                if (list[0].mr > list[1].mr)
                {
                    Next = false;
                    result = -4;
                }
                else if (list[0].mr < list[1].mr)
                {
                    Next = true;
                    result = -4;
                }
                else
                {
                    result = 0;
                }
                if (result == 0)
                { }
                else
                {
                    for (int i = 1; i < 12; i++)
                    {
                        if (Next == false)
                        {
                            if (list[i].mr < list[i + 1].mr)
                            {
                                Next = true;
                            }
                            else
                            {
                                result = 0;
                                break;
                            }
                        }
                        else
                        {
                            if (list[i].mr > list[i + 1].mr)
                            {
                                Next = false;
                            }
                            else
                            {
                                result = 0;
                                break;
                            }
                        }
                    }
                    if (result == -4)
                    {
                        CalFault entity = new CalFault();
                        entity.CalFaultID = CommonHelper.GetGuid;
                        entity.ProductCalID = ProductCalID;
                        entity.ProductCalDID = list[0].ProductCalDID;
                        entity.FaultContent = "连续14点交替上下";
                        entity.FaultFrom = "MR";
                        entity.CreateDate = DateTime.Now;
                        entity.CreatrBy = ManageProvider.Provider.Current().UserName;
                        FaultRe.Insert(entity);
                    }

                }
            }
            

            return result;
        }

        /// <summary>
        /// 连续3个点中有2个点落在中心线同一侧的B区以外
        /// </summary>
        /// <param name="list"></param>
        /// <param name="ProductCalID"></param>
        /// <returns></returns>
        public int Case5(List<MRPictureDTO> list, string ProductCalID, bool IsCheck)
        {
            if (IsCheck == false)
            {
                return 0;
            }
            int result = 0;
            int temp = 0;
            decimal XupAverage = list[0].XUpperLimit;
            decimal XlowAverage = list[0].XLowLimit;
            if (list[0].x > XupAverage)
            {
                temp = temp + 1;
            }
            if (list[1].x > XupAverage)
            {
                temp = temp + 1;
            }
            if (list[2].x > XupAverage)
            {
                temp = temp + 1;
            }
            if (temp >= 2)
            {
                CalFault entity = new CalFault();
                entity.CalFaultID = CommonHelper.GetGuid;
                entity.ProductCalID = ProductCalID;
                entity.ProductCalDID = list[0].ProductCalDID;
                entity.FaultContent = "连续3个点中有2个点落在中心线上侧的B区以外";
                entity.FaultFrom = "X";
                entity.CreateDate = DateTime.Now;
                entity.CreatrBy = ManageProvider.Provider.Current().UserName;
                FaultRe.Insert(entity);
                result = -5;
                temp = 0;
            }
            if (list[0].x < XlowAverage)
            {
                temp = temp - 1;
            }
            if (list[1].x < XlowAverage)
            {
                temp = temp - 1;
            }
            if (list[2].x < XlowAverage)
            {
                temp = temp - 1;
            }
            if (temp <= -2)
            {
                CalFault entity = new CalFault();
                entity.CalFaultID = CommonHelper.GetGuid;
                entity.ProductCalID = ProductCalID;
                entity.ProductCalDID = list[0].ProductCalDID;
                entity.FaultContent = "连续3个点中有2个点落在中心线下侧的B区以外";
                entity.FaultFrom = "X";
                entity.CreateDate = DateTime.Now;
                entity.CreatrBy = ManageProvider.Provider.Current().UserName;
                FaultRe.Insert(entity);
                result = -5;
                temp = 0;
            }
            if(list.Count>3)
            {
                temp = 0;
                decimal RupAverage = list[0].RUpperLimit;
                decimal RlowAverage = list[0].RLowLimit;
                if (list[0].mr > RupAverage)
                {
                    temp = temp + 1;
                }
                if (list[1].mr > RupAverage)
                {
                    temp = temp + 1;
                }
                if (list[2].mr > RupAverage)
                {
                    temp = temp + 1;
                }
                if (temp >= 2)
                {
                    CalFault entity = new CalFault();
                    entity.CalFaultID = CommonHelper.GetGuid;
                    entity.ProductCalID = ProductCalID;
                    entity.ProductCalDID = list[0].ProductCalDID;
                    entity.FaultContent = "连续3个点中有2个点落在中心线上侧的B区以外";
                    entity.FaultFrom = "MR";
                    entity.CreateDate = DateTime.Now;
                    entity.CreatrBy = ManageProvider.Provider.Current().UserName;
                    FaultRe.Insert(entity);
                    result = -5;
                    temp = 0;
                }
                if (list[0].mr < RlowAverage)
                {
                    temp = temp - 1;
                }
                if (list[1].mr < RlowAverage)
                {
                    temp = temp - 1;
                }
                if (list[2].mr < RlowAverage)
                {
                    temp = temp - 1;
                }
                if (temp <= -2)
                {
                    CalFault entity = new CalFault();
                    entity.CalFaultID = CommonHelper.GetGuid;
                    entity.ProductCalID = ProductCalID;
                    entity.ProductCalDID = list[0].ProductCalDID;
                    entity.FaultContent = "连续3个点中有2个点落在中心线下侧的B区以外";
                    entity.FaultFrom = "MR";
                    entity.CreateDate = DateTime.Now;
                    entity.CreatrBy = ManageProvider.Provider.Current().UserName;
                    FaultRe.Insert(entity);
                    result = -5;
                    temp = 0;
                }
            }

            return result;
        }
        /// <summary>
        /// 连续5个点中有4个点落在中心线同一侧的C区以外
        /// </summary>
        /// <param name="list"></param>
        /// <param name="ProductCalID"></param>
        /// <returns></returns>
        /// 
        public int Case6(List<MRPictureDTO> list, string ProductCalID, bool IsCheck)
        {
            if (IsCheck == false)
            {
                return 0;
            }
            int result = 0;
            int temp = 0;
            decimal XupAverage = (list[0].XUpperLimit - list[0].XCenterLine) / 2;
            decimal XlowAverage = (list[0].XLowLimit - list[0].XCenterLine) / 2;
            if (list[0].x > XupAverage)
            {
                temp = temp + 1;
            }
            if (list[1].x > XupAverage)
            {
                temp = temp + 1;
            }
            if (list[2].x > XupAverage)
            {
                temp = temp + 1;
            }
            if (list[3].x > XupAverage)
            {
                temp = temp + 1;
            }
            if (list[4].x > XupAverage)
            {
                temp = temp + 1;
            }
            if (temp >= 4)
            {
                CalFault entity = new CalFault();
                entity.CalFaultID = CommonHelper.GetGuid;
                entity.ProductCalID = ProductCalID;
                entity.ProductCalDID = list[0].ProductCalDID;
                entity.FaultContent = "连续5个点中有4个点落在中心线上的C区以外";
                entity.FaultFrom = "X";
                entity.CreateDate = DateTime.Now;
                entity.CreatrBy = ManageProvider.Provider.Current().UserName;
                FaultRe.Insert(entity);
                result = -6;
                temp = 0;
            }
            if (list[0].x < XlowAverage)
            {
                temp = temp - 1;
            }
            if (list[1].x < XlowAverage)
            {
                temp = temp - 1;
            }
            if (list[2].x < XlowAverage)
            {
                temp = temp - 1;
            }
            if (list[3].x < XlowAverage)
            {
                temp = temp - 1;
            }
            if (list[4].x < XlowAverage)
            {
                temp = temp - 1;
            }
            if (temp <= -4)
            {
                CalFault entity = new CalFault();
                entity.CalFaultID = CommonHelper.GetGuid;
                entity.ProductCalID = ProductCalID;
                entity.ProductCalDID = list[0].ProductCalDID;
                entity.FaultContent = "连续5个点中有4个点落在中心线下的C区以外";
                entity.FaultFrom = "X";
                entity.CreateDate = DateTime.Now;
                entity.CreatrBy = ManageProvider.Provider.Current().UserName;
                FaultRe.Insert(entity);
                result = -6;
            }

            if(list.Count>5)
            {
                temp = 0;
                decimal RupAverage = (list[0].RUpperLimit - list[0].RCenterLine) / 2;
                decimal RlowAverage = (list[0].RLowLimit - list[0].RCenterLine) / 2;
                if (list[0].mr > RupAverage)
                {
                    temp = temp + 1;
                }
                if (list[1].mr > RupAverage)
                {
                    temp = temp + 1;
                }
                if (list[2].mr > RupAverage)
                {
                    temp = temp + 1;
                }
                if (list[3].mr > RupAverage)
                {
                    temp = temp + 1;
                }
                if (list[4].mr > RupAverage)
                {
                    temp = temp + 1;
                }
                if (temp >= 4)
                {
                    CalFault entity = new CalFault();
                    entity.CalFaultID = CommonHelper.GetGuid;
                    entity.ProductCalID = ProductCalID;
                    entity.ProductCalDID = list[0].ProductCalDID;
                    entity.FaultContent = "连续5个点中有4个点落在中心线上的C区以外";
                    entity.FaultFrom = "MR";
                    entity.CreateDate = DateTime.Now;
                    entity.CreatrBy = ManageProvider.Provider.Current().UserName;
                    FaultRe.Insert(entity);
                    result = -6;
                    temp = 0;
                }
                if (list[0].mr < RupAverage)
                {
                    temp = temp - 1;
                }
                if (list[1].mr < RupAverage)
                {
                    temp = temp - 1;
                }
                if (list[2].mr < RupAverage)
                {
                    temp = temp - 1;
                }
                if (list[3].mr < RupAverage)
                {
                    temp = temp - 1;
                }
                if (list[4].mr < RupAverage)
                {
                    temp = temp - 1;
                }
                if (temp <= -4)
                {
                    CalFault entity = new CalFault();
                    entity.CalFaultID = CommonHelper.GetGuid;
                    entity.ProductCalID = ProductCalID;
                    entity.ProductCalDID = list[0].ProductCalDID;
                    entity.FaultContent = "连续5个点中有4个点落在中心线下的C区以外";
                    entity.FaultFrom = "MR";
                    entity.CreateDate = DateTime.Now;
                    entity.CreatrBy = ManageProvider.Provider.Current().UserName;
                    FaultRe.Insert(entity);
                    result = -6;
                }
            }
            return result;
        }
        /// <summary>
        /// 连续15个点落在中心线两侧的C区以内
        /// </summary>
        /// <param name="list"></param>
        /// <param name="ProductCalID"></param>
        /// <returns></returns>
        public int Case7(List<MRPictureDTO> list, string ProductCalID, bool IsCheck)
        {
            if (IsCheck == false)
            {
                return 0;
            }
            int result = 0;
            bool IsIn = true;
            decimal XupAverage = (list[0].XUpperLimit - list[0].XCenterLine) / 2;
            decimal XlowAverage = (list[0].XLowLimit - list[0].XCenterLine) / 2;
            for (int i = 0; i < 15; i++)
            {
                if (list[i].x > XupAverage || list[i].x < XlowAverage)
                {
                    IsIn = false;
                    break;
                }
            }
            if (IsIn == true)
            {
                CalFault entity = new CalFault();
                entity.CalFaultID = CommonHelper.GetGuid;
                entity.ProductCalID = ProductCalID;
                entity.ProductCalDID = list[0].ProductCalDID;
                entity.FaultContent = "连续15个点落在中心线两侧的C区以内";
                entity.FaultFrom = "X";
                entity.CreateDate = DateTime.Now;
                entity.CreatrBy = ManageProvider.Provider.Current().UserName;
                FaultRe.Insert(entity);
                result = -7;
            }
            if(list.Count>15)
            {
                IsIn = true;
                decimal RupAverage = (list[0].RUpperLimit - list[0].RCenterLine) / 2;
                decimal RlowAverage = (list[0].RLowLimit - list[0].RCenterLine) / 2;
                for (int i = 0; i < 15; i++)
                {
                    if (list[i].mr > RupAverage || list[i].mr < RlowAverage)
                    {
                        IsIn = false;
                        break;
                    }
                }
                if (IsIn == true)
                {
                    CalFault entity = new CalFault();
                    entity.CalFaultID = CommonHelper.GetGuid;
                    entity.ProductCalID = ProductCalID;
                    entity.ProductCalDID = list[0].ProductCalDID;
                    entity.FaultContent = "连续15个点落在中心线两侧的C区以内";
                    entity.FaultFrom = "R";
                    entity.CreateDate = DateTime.Now;
                    entity.CreatrBy = ManageProvider.Provider.Current().UserName;
                    FaultRe.Insert(entity);
                    result = -7;
                }
            }
            return result;
        }
        /// <summary>
        /// 连续8个点落在中心线两侧且无一在C区
        /// </summary>
        /// <param name="list"></param>
        /// <param name="ProductCalID"></param>
        /// <returns></returns>
        public int Case8(List<MRPictureDTO> list, string ProductCalID, bool IsCheck)
        {
            if (IsCheck == false)
            {
                return 0;
            }
            int result = 0;
            bool isIn = false;
            decimal XupAverage = (list[0].XUpperLimit - list[0].XCenterLine) / 2;
            decimal XlowAverage = (list[0].XLowLimit - list[0].XCenterLine) / 2;


            decimal RupAverage = (list[0].RUpperLimit - list[0].RCenterLine) / 2;
            decimal RlowAverage = (list[0].RLowLimit - list[0].RCenterLine) / 2;

            for (int i = 0; i < 8; i++)
            {
                if (list[i].x < XupAverage && list[i].x > XlowAverage)
                {
                    isIn = true;
                    break;
                }
            }
            if (isIn == false)
            {
                CalFault entity = new CalFault();
                entity.CalFaultID = CommonHelper.GetGuid;
                entity.ProductCalID = ProductCalID;
                entity.ProductCalDID = list[0].ProductCalDID;
                entity.FaultContent = "连续8个点落在中心线两侧且无一在C区内";
                entity.FaultFrom = "X";
                entity.CreateDate = DateTime.Now;
                entity.CreatrBy = ManageProvider.Provider.Current().UserName;
                FaultRe.Insert(entity);
                result = -8;
            }
            if(list.Count>8)
            {
                isIn = false;
                for (int i = 0; i < 8; i++)
                {
                    if (list[i].mr < RupAverage && list[i].mr > RlowAverage)
                    {
                        isIn = true;
                        break;
                    }
                }
                if (isIn == false)
                {
                    CalFault entity = new CalFault();
                    entity.CalFaultID = CommonHelper.GetGuid;
                    entity.ProductCalID = ProductCalID;
                    entity.ProductCalDID = list[0].ProductCalDID;
                    entity.FaultContent = "连续8个点落在中心线两侧且无一在C区内";
                    entity.FaultFrom = "MR";
                    entity.CreateDate = DateTime.Now;
                    entity.CreatrBy = ManageProvider.Provider.Current().UserName;
                    FaultRe.Insert(entity);
                    result = -8;
                }
            }
            return result;
        }

        /// <summary>
        /// 计算MR图的CPK，方法和XBAR差不多，就是R的计算需要调整
        /// </summary>
        /// <param name="ProductCalID"></param>
        /// <param name="StartDate"></param>
        /// <param name="EndDate"></param>
        /// <param name="re"></param>
        /// <returns></returns>
        public static string CalCPK(string ProductCalID,string StartDate,string EndDate)
        {
            try
            {
                MRCalResultBll bll = new MRCalResultBll();
                if (CommonHelper.IsEmpty(StartDate))
                {
                    StartDate = DateTime.Now.AddDays(-30).ToString();
                }
                if (CommonHelper.IsEmpty(EndDate))
                {
                    EndDate = DateTime.Now.ToString();
                }
                string sql = @"  select  CONVERT(varchar(100), b.StartDate, 120)+'~'+CONVERT(varchar(100), b.EndDate, 8) as riqi,
CONVERT(varchar(100), b.StartDate, 120) as sdate,
CONVERT(varchar(100), b.EndDate, 8) as edate,
AVG(InputValue) as x,
c.XLowLimit,c.XUpperLimit,c.XCenterLine,
c.RLowLimit,c.RUpperLimit,c.RCenterLine,
c.StandardLowLimit,c.StandardUpperLimit,c.StandardCenterLine
from ProductCalData a 
left join ProductCalDetail  b on a.ProductCalDID=b.ProductCalDID 
left join ProductCal c on b.ProductCalID=c.ProductCalID
where a.ProductCalID='{0}'  and cast(b.StartDate as date)>=cast('{1}' as date) and cast(b.StartDate as date)<=cast('{2}' as date) and  b.State='已完成'
group by a.ProductCalDID,b.StartDate,b.EndDate,c.XLowLimit,c.XUpperLimit,c.XCenterLine,c.RLowLimit,c.RUpperLimit,c.RCenterLine,
c.StandardLowLimit,c.StandardUpperLimit,c.StandardCenterLine 
order by StartDate ";
                sql = string.Format(sql, ProductCalID, StartDate, EndDate);
                Repository<MRPictureDTO> re = new Repository<MRPictureDTO>();
                List<MRPictureDTO> list = re.FindListBySql(sql);
                list = bll.GetMRList(list);
                //对list进行求和，求平均等操作
                var xbarAverage = list.Average(q => q.x);
                var rSum = list.Sum(q => q.mr);
                var rAverage = rSum / (list.Count - 1);
                decimal fenzi1 = list[0].StandardUpperLimit - xbarAverage;
                if (fenzi1 < 0)
                {
                    fenzi1 = fenzi1 * -1;
                }
                decimal fenzi2 = xbarAverage - list[0].StandardUpperLimit;
                if (fenzi2 < 0)
                {
                    fenzi2 = fenzi2 * -1;
                }
                decimal fenzi;
                if (fenzi1 > fenzi2)
                {
                    fenzi = fenzi2;
                }
                else
                {
                    fenzi = fenzi1;
                }
                decimal fenmu = 3 * rAverage / CalResultBll.CpkCanshu(2);
                return (fenzi / fenmu).ToString("0.00");
            }
            catch(Exception ex)
            {
                return "0";
            }
            
        }
    }
}