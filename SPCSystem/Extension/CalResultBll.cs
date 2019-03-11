using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Repository;
using Utilities;
using System.Data;

namespace SPCSystem
{
    public class CalResultBll
    {
        Repository<CalFault> FaultRe = new Repository<CalFault>();
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
                Repository<CheckFaultDTO> re = new Repository<CheckFaultDTO>();
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


                string sql = @"select top 15 b.ProductCalDID,a.XCenterLine,a.XLowLimit,a.XUpperLimit,a.RCenterLine,a.RLowLimit,a.RUpperLimit,avg(c.InputValue) as xValue
,MAX(InputValue)-MIN(InputValue) as rValue
From ProductCal a
left join ProductCalDetail b on a.ProductCalID=b.ProductCalID
left join ProductCalData c on b.ProductCalDID=c.ProductCalDID
where a.ProductCalID='{0}'
group by b.ProductCalDID,a.XCenterLine,a.XLowLimit,a.XUpperLimit,a.RCenterLine,a.RLowLimit,a.RUpperLimit,b.StartDate
order by b.StartDate desc";
                sql = string.Format(sql, ProductCalID);
                List<CheckFaultDTO> list = re.FindListBySql(sql);
                if (list.Count <= 0)
                {
                    //异常
                    return "-9";
                }
                //小于3只需要判断case1
                if (list.Count < 3)
                {
                    if (Case1(list, ProductCalID, check1) == -1)
                    {
                        return "-1";
                    }
                    else
                    {
                        return "1";
                    }
                }
                else
                {
                    if (list.Count >= 3 && list.Count < 5)
                    {
                        //判断case1+case5
                        return Case1(list, ProductCalID, check1) + ":" + Case5(list, ProductCalID, check5);
                    }
                    else if (list.Count >= 5 && list.Count < 6)
                    {
                        //判断case1+case5+case6
                        return Case1(list, ProductCalID, check1) + ":" + Case5(list, ProductCalID, check5) + ":" + Case6(list, ProductCalID, check6);
                    }
                    else if (list.Count >= 6 && list.Count < 7)
                    {
                        //判断case1+case3+case5+case6
                        int case1 = Case1(list, ProductCalID, check1);
                        int case3 = Case3(list, ProductCalID, check3);
                        int case5 = Case5(list, ProductCalID, check5);
                        return case1.ToString() + ":" + case3.ToString() + ":" + case5.ToString() + ":" + Case6(list, ProductCalID, check6);

                    }
                    else
                    {
                        //判断case1+case2+case3+5+6
                        if (list.Count >= 7 && list.Count < 8)
                        {
                            int IsIn = Case1(list, ProductCalID, check1);
                            int IsContinous = Case3(list, ProductCalID, check3);
                            int IsSameDirection = Case2(list, ProductCalID, check2);
                            return IsIn.ToString() + ":" + IsSameDirection.ToString() + ":" + IsContinous.ToString() + ":" + Case5(list, ProductCalID, check5) + ":" + Case6(list, ProductCalID, check6);
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
                }
            }
            catch
            {
                return "计算检验结果时出错了";
            }
            //else if(list.Count>=6)
            //{
            //    if(list)
            //}
            //return "0";
        }

        /// <summary>
        /// 检测一点落在控制限以外
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public int Case1(List<CheckFaultDTO> list, string ProductCalID, bool IsCheck)
        {
            if (IsCheck == false)
            {
                return 0;
            }
            int result = 0;
            if (list[0].xValue > list[0].XUpperLimit || list[0].xValue < list[0].XLowLimit)
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
            if (list[0].rValue > list[0].RUpperLimit || list[0].xValue < list[0].RLowLimit)
            {
                //插入问题
                CalFault entity = new CalFault();
                entity.CalFaultID = CommonHelper.GetGuid;
                entity.ProductCalID = ProductCalID;
                entity.ProductCalDID = list[0].ProductCalDID;
                entity.FaultContent = "一点落在控制限以外";
                entity.FaultFrom = "R";
                entity.CreateDate = DateTime.Now;
                entity.CreatrBy = ManageProvider.Provider.Current().UserName;
                FaultRe.Insert(entity);
                result = -1;
            }
            //else
            //{
            //    //正常返回0
            //    result = 0;
            //}
            return result;
        }
        /// <summary>
        /// 连续6点递增或递减
        /// </summary>
        /// <param name="list"></param>
        /// <param name="ProductCalID"></param>
        /// <returns></returns>
        public int Case3(List<CheckFaultDTO> list, string ProductCalID, bool IsCheck)
        {
            if (IsCheck == false)
            {
                return 0;
            }
            int result = 0;
            if (list[0].xValue < list[1].xValue && list[1].xValue < list[2].xValue && list[2].xValue < list[3].xValue && list[3].xValue < list[4].xValue && list[4].xValue < list[5].xValue)
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
            if (list[0].xValue > list[1].xValue && list[1].xValue > list[2].xValue && list[2].xValue > list[3].xValue && list[3].xValue > list[4].xValue && list[4].xValue > list[5].xValue)
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
            if (list[0].rValue < list[1].rValue && list[1].rValue < list[2].rValue && list[2].rValue < list[3].rValue && list[3].rValue < list[4].rValue && list[4].rValue < list[5].rValue)
            {
                CalFault entity = new CalFault();
                entity.CalFaultID = CommonHelper.GetGuid;
                entity.ProductCalID = ProductCalID;
                entity.ProductCalDID = list[0].ProductCalDID;
                entity.FaultContent = "连续6点递增";
                entity.FaultFrom = "R";
                entity.CreateDate = DateTime.Now;
                entity.CreatrBy = ManageProvider.Provider.Current().UserName;
                FaultRe.Insert(entity);
                result = -3;
            }
            if (list[0].rValue > list[1].rValue && list[1].rValue > list[2].rValue && list[2].rValue > list[3].rValue && list[3].rValue > list[4].xValue && list[4].rValue > list[5].rValue)
            {
                CalFault entity = new CalFault();
                entity.CalFaultID = CommonHelper.GetGuid;
                entity.ProductCalID = ProductCalID;
                entity.ProductCalDID = list[0].ProductCalDID;
                entity.FaultContent = "连续6点递增";
                entity.FaultFrom = "R";
                entity.CreateDate = DateTime.Now;
                entity.CreatrBy = ManageProvider.Provider.Current().UserName;
                FaultRe.Insert(entity);
                result = -3;
            }
            return result;
        }
        /// <summary>
        /// 判断是否连续7点落在中心线的同一侧
        /// </summary>
        /// <param name="list"></param>
        /// <param name="ProductCalID"></param>
        /// <returns></returns>
        public int Case2(List<CheckFaultDTO> list, string ProductCalID, bool IsCheck)
        {
            if (IsCheck == false)
            {
                return 0;
            }
            int result = 0;
            //1表示上侧-1表示下侧
            int AboveOrBelow = 0;
            if (list[0].xValue > list[0].XCenterLine)
            {
                AboveOrBelow = 1;
                for (int i = 1; i < 7; i++)
                {
                    if (list[i].xValue <= list[0].XCenterLine)
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
            else if (list[0].xValue < list[0].XCenterLine)
            {
                AboveOrBelow = -1;
                for (int i = 1; i < 7; i++)
                {
                    if (list[i].xValue >= list[0].XCenterLine)
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
            if (list[0].rValue > list[0].RCenterLine)
            {
                AboveOrBelow = 1;
                for (int i = 1; i < 7; i++)
                {
                    if (list[i].rValue <= list[0].RCenterLine)
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
                    entity.FaultFrom = "R";
                    entity.CreateDate = DateTime.Now;
                    entity.CreatrBy = ManageProvider.Provider.Current().UserName;
                    FaultRe.Insert(entity);
                    result = -2;
                }
            }
            else if (list[0].rValue < list[0].RCenterLine)
            {
                AboveOrBelow = -1;
                for (int i = 1; i < 7; i++)
                {
                    if (list[i].rValue >= list[0].RCenterLine)
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
                    entity.FaultFrom = "R";
                    entity.CreateDate = DateTime.Now;
                    entity.CreatrBy = ManageProvider.Provider.Current().UserName;
                    FaultRe.Insert(entity);
                    result = -2;
                }
            }
            else
            {

            }

            return result;


        }
        /// <summary>
        /// 判断连续14点是否交替上下
        /// </summary>
        /// <param name="list"></param>
        /// <param name="ProductCalID"></param>
        /// <returns></returns>
        public int Case4(List<CheckFaultDTO> list, string ProductCalID, bool IsCheck)
        {
            if (IsCheck == false)
            {
                return 0;
            }
            int result = 0;
            //判断下一次判断是大于还是小于
            bool Next = true;
            if (list[0].xValue > list[1].xValue)
            {
                Next = false;
                result = -4;
            }
            else if (list[0].xValue < list[1].xValue)
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
                        if (list[i].xValue < list[i + 1].xValue)
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
                        if (list[i].xValue > list[i + 1].xValue)
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

            //判断R
            if (list[0].rValue > list[1].rValue)
            {
                Next = false;
                result = -4;
            }
            else if (list[0].rValue < list[1].rValue)
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
                        if (list[i].rValue < list[i + 1].rValue)
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
                        if (list[i].rValue > list[i + 1].rValue)
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
                    entity.FaultFrom = "R";
                    entity.CreateDate = DateTime.Now;
                    entity.CreatrBy = ManageProvider.Provider.Current().UserName;
                    FaultRe.Insert(entity);
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
        public int Case5(List<CheckFaultDTO> list, string ProductCalID, bool IsCheck)
        {
            if (IsCheck == false)
            {
                return 0;
            }
            int result = 0;
            int temp = 0;
            decimal XupAverage = list[0].XUpperLimit;
            decimal XlowAverage = list[0].XLowLimit;
            if (list[0].xValue > XupAverage)
            {
                temp = temp + 1;
            }
            if (list[1].xValue > XupAverage)
            {
                temp = temp + 1;
            }
            if (list[2].xValue > XupAverage)
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
            if (list[0].xValue < XlowAverage)
            {
                temp = temp - 1;
            }
            if (list[1].xValue < XlowAverage)
            {
                temp = temp - 1;
            }
            if (list[2].xValue < XlowAverage)
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
            temp = 0;
            decimal RupAverage = list[0].RUpperLimit;
            decimal RlowAverage = list[0].RLowLimit;
            if (list[0].rValue > RupAverage)
            {
                temp = temp + 1;
            }
            if (list[1].rValue > RupAverage)
            {
                temp = temp + 1;
            }
            if (list[2].rValue > RupAverage)
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
                entity.FaultFrom = "R";
                entity.CreateDate = DateTime.Now;
                entity.CreatrBy = ManageProvider.Provider.Current().UserName;
                FaultRe.Insert(entity);
                result = -5;
                temp = 0;
            }
            if (list[0].rValue < RlowAverage)
            {
                temp = temp - 1;
            }
            if (list[1].rValue < RlowAverage)
            {
                temp = temp - 1;
            }
            if (list[2].rValue < RlowAverage)
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
                entity.FaultFrom = "R";
                entity.CreateDate = DateTime.Now;
                entity.CreatrBy = ManageProvider.Provider.Current().UserName;
                FaultRe.Insert(entity);
                result = -5;
                temp = 0;
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
        public int Case6(List<CheckFaultDTO> list, string ProductCalID, bool IsCheck)
        {
            if (IsCheck == false)
            {
                return 0;
            }
            int result = 0;
            int temp = 0;
            decimal XupAverage = (list[0].XUpperLimit - list[0].XCenterLine) / 2;
            decimal XlowAverage = (list[0].XLowLimit - list[0].XCenterLine) / 2;
            if (list[0].xValue > XupAverage)
            {
                temp = temp + 1;
            }
            if (list[1].xValue > XupAverage)
            {
                temp = temp + 1;
            }
            if (list[2].xValue > XupAverage)
            {
                temp = temp + 1;
            }
            if (list[3].xValue > XupAverage)
            {
                temp = temp + 1;
            }
            if (list[4].xValue > XupAverage)
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
            if (list[0].xValue < XlowAverage)
            {
                temp = temp - 1;
            }
            if (list[1].xValue < XlowAverage)
            {
                temp = temp - 1;
            }
            if (list[2].xValue < XlowAverage)
            {
                temp = temp - 1;
            }
            if (list[3].xValue < XlowAverage)
            {
                temp = temp - 1;
            }
            if (list[4].xValue < XlowAverage)
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

            temp = 0;
            decimal RupAverage = (list[0].RUpperLimit - list[0].RCenterLine) / 2;
            decimal RlowAverage = (list[0].RLowLimit - list[0].RCenterLine) / 2;
            if (list[0].rValue > RupAverage)
            {
                temp = temp + 1;
            }
            if (list[1].rValue > RupAverage)
            {
                temp = temp + 1;
            }
            if (list[2].rValue > RupAverage)
            {
                temp = temp + 1;
            }
            if (list[3].rValue > RupAverage)
            {
                temp = temp + 1;
            }
            if (list[4].rValue > RupAverage)
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
                entity.FaultFrom = "R";
                entity.CreateDate = DateTime.Now;
                entity.CreatrBy = ManageProvider.Provider.Current().UserName;
                FaultRe.Insert(entity);
                result = -6;
                temp = 0;
            }
            if (list[0].rValue < RupAverage)
            {
                temp = temp - 1;
            }
            if (list[1].rValue < RupAverage)
            {
                temp = temp - 1;
            }
            if (list[2].rValue < RupAverage)
            {
                temp = temp - 1;
            }
            if (list[3].rValue < RupAverage)
            {
                temp = temp - 1;
            }
            if (list[4].rValue < RupAverage)
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
                entity.FaultFrom = "R";
                entity.CreateDate = DateTime.Now;
                entity.CreatrBy = ManageProvider.Provider.Current().UserName;
                FaultRe.Insert(entity);
                result = -6;
            }
            return result;
        }
        /// <summary>
        /// 连续15个点落在中心线两侧的C区以内
        /// </summary>
        /// <param name="list"></param>
        /// <param name="ProductCalID"></param>
        /// <returns></returns>
        public int Case7(List<CheckFaultDTO> list, string ProductCalID, bool IsCheck)
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
                if (list[i].xValue > XupAverage || list[i].xValue < XlowAverage)
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
            IsIn = true;
            decimal RupAverage = (list[0].RUpperLimit - list[0].RCenterLine) / 2;
            decimal RlowAverage = (list[0].RLowLimit - list[0].RCenterLine) / 2;
            for (int i = 0; i < 15; i++)
            {
                if (list[i].rValue > RupAverage || list[i].rValue < RlowAverage)
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
            return result;
        }
        /// <summary>
        /// 连续8个点落在中心线两侧且无一在C区
        /// </summary>
        /// <param name="list"></param>
        /// <param name="ProductCalID"></param>
        /// <returns></returns>
        public int Case8(List<CheckFaultDTO> list, string ProductCalID, bool IsCheck)
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
                if (list[i].xValue < XupAverage && list[i].xValue > XlowAverage)
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
            isIn = false;
            for (int i = 0; i < 8; i++)
            {
                if (list[i].rValue < RupAverage && list[i].rValue > RlowAverage)
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
                entity.FaultFrom = "R";
                entity.CreateDate = DateTime.Now;
                entity.CreatrBy = ManageProvider.Provider.Current().UserName;
                FaultRe.Insert(entity);
                result = -8;
            }
            return result;
        }
        /// <summary>
        /// 显示指定的日期内的行转列拼接以后的table
        /// </summary>
        /// <param name="ProductCalID"></param>
        /// <param name="StartDate"></param>
        /// <param name="EndDate"></param>
        /// <returns></returns>
        public static string DetailTable(string ProductCalID, string StartDate, string EndDate, Repository<ProductCal> re)
        {
            string ResultTable = "<table class=\"table table-dark table-bordered\">";
            if (CommonHelper.IsEmpty(StartDate))
            {
                StartDate = DateTime.Now.AddDays(-30).ToString();
            }
            if (CommonHelper.IsEmpty(EndDate))
            {
                EndDate = DateTime.Now.ToString();
            }
            string sqlData = @"select a.GroupNum,CONVERT(varchar(100), b.StartDate, 120)+'~'+CONVERT(varchar(100), b.EndDate, 8) as durdate,c.InputValue 
from ProductCal a 
left join ProductCalDetail b on a.ProductCalID=b.ProductCalID
left join ProductCalData c on b.ProductCalDID=c.ProductCalDID
where a.ProductCalID='{0}' and cast(b.StartDate as date)>=cast('{1}' as date) and cast(b.StartDate as date)<=cast('{2}' as date)  and  b.State='已完成'
order by CONVERT(varchar(100), b.StartDate, 120)+'~'+CONVERT(varchar(100), b.EndDate, 8)";
            sqlData = string.Format(sqlData, ProductCalID, StartDate, EndDate);
            string sqlDate = @"select distinct CONVERT(varchar(100), b.StartDate, 120)+'~'+CONVERT(varchar(100), b.EndDate, 8) as durdate,UpdateMan
from ProductCal a 
left join ProductCalDetail b on a.ProductCalID=b.ProductCalID
left join ProductCalData c on b.ProductCalDID=c.ProductCalDID
where a.ProductCalID='{0}' and cast(b.StartDate as date)>=cast('{1}' as date) and cast(b.StartDate as date)<=cast('{2}' as date)  and  b.State='已完成'
order by CONVERT(varchar(100), b.StartDate, 120)+'~'+CONVERT(varchar(100), b.EndDate, 8)";
            sqlDate = string.Format(sqlDate, ProductCalID, StartDate, EndDate);
            DataTable dtData = re.FindTableBySql(sqlData);
            DataTable dtDate = re.FindTableBySql(sqlDate);
            string NameTemp = "";
            //标题列，日期
            if (dtDate.Rows.Count > 0)
            {
                ResultTable += "<tr>";
                NameTemp += "<tr>";
                for (int i = 0; i < dtDate.Rows.Count; i++)
                {
                    ResultTable += "<th>" + dtDate.Rows[i][0] + "</th>";
                    NameTemp += "<th>" + dtDate.Rows[i][1] + "</th>";
                }
                ResultTable += "</tr>";
                NameTemp += "</tr>";
            }
            ResultTable = ResultTable + NameTemp;
            if (dtData.Rows.Count > 0)
            {
                int GroupNum = Convert.ToInt32(dtData.Rows[0][0].ToString());
                //按照groupnum进行循环，比如15条数据，groupnum=3，则循环5次

                for (int i = 0; i < GroupNum; i++)
                {
                    ResultTable += "<tr>";
                    for (int j = 0; j < dtData.Rows.Count / GroupNum; j++)
                    {
                        ResultTable += "<td>" + dtData.Rows[i + j][2].ToString() + "</td>";
                    }
                    ResultTable += "</tr>";
                    // ResultTable += "<td>" + dtData.Rows[i][2].ToString() + "</td>";
                }

            }
            ResultTable += "</table>";
            return ResultTable;
        }

        /// <summary>
        /// 显示该主表下面所有的已录入数据
        /// </summary>
        /// <param name="ProductCalID"></param>
        /// <param name="re"></param>
        /// <returns></returns>
        public static string DefaultDetailTable(string ProductCalID, Repository<ProductCal> re)
        {
            string ResultTable = "<table class=\"table table-dark table-bordered\">";
            string sqlData = @"select a.GroupNum,CONVERT(varchar(100), b.StartDate, 120)+'~'+CONVERT(varchar(100), b.EndDate, 8) as durdate,c.InputValue 
from ProductCal a 
left join ProductCalDetail b on a.ProductCalID=b.ProductCalID
left join ProductCalData c on b.ProductCalDID=c.ProductCalDID
where a.ProductCalID='{0}'  and  b.State='已完成'
order by CONVERT(varchar(100), b.StartDate, 120)+'~'+CONVERT(varchar(100), b.EndDate, 8)";
            sqlData = string.Format(sqlData, ProductCalID);
            string sqlDate = @"select distinct CONVERT(varchar(100), b.StartDate, 120)+'~'+CONVERT(varchar(100), b.EndDate, 8) as durdate,b.UpdateMan
from ProductCal a 
left join ProductCalDetail b on a.ProductCalID=b.ProductCalID
left join ProductCalData c on b.ProductCalDID=c.ProductCalDID
where a.ProductCalID='{0}'  and  b.State='已完成'
order by CONVERT(varchar(100), b.StartDate, 120)+'~'+CONVERT(varchar(100), b.EndDate, 8)";
            sqlDate = string.Format(sqlDate, ProductCalID);
            DataTable dtData = re.FindTableBySql(sqlData);
            DataTable dtDate = re.FindTableBySql(sqlDate);

            string NameTemp = "";
            //标题列，日期
            if (dtDate.Rows.Count > 0)
            {
                ResultTable += "<tr>";
                NameTemp += "<tr>";
                for (int i = 0; i < dtDate.Rows.Count; i++)
                {
                    ResultTable += "<th>" + dtDate.Rows[i][0] + "</th>";
                    NameTemp += "<th>" + dtDate.Rows[i][1] + "</th>";
                }
                ResultTable += "</tr>";
                NameTemp += "</tr>";
            }
            ResultTable = ResultTable + NameTemp;
            if (dtData.Rows.Count > 0)
            {
                int GroupNum = Convert.ToInt32(dtData.Rows[0][0].ToString());
                //按照groupnum进行循环，比如15条数据，groupnum=3，则循环5次

                for (int i = 0; i < GroupNum; i++)
                {
                    ResultTable += "<tr>";
                    for (int j = 0; j < dtData.Rows.Count / GroupNum; j++)
                    {
                        ResultTable += "<td>" + dtData.Rows[i + j][2].ToString() + "</td>";
                    }
                    ResultTable += "</tr>";
                    // ResultTable += "<td>" + dtData.Rows[i][2].ToString() + "</td>";
                }

            }
            ResultTable += "</table>";
            return ResultTable;
        }

        /// <summary>
        /// 检测异常的结果拼接table
        /// </summary>
        /// <param name="ProductCalID"></param>
        /// <param name="StartDate"></param>
        /// <param name="EndDate"></param>
        /// <param name="re"></param>
        /// <returns></returns>
        public static string FaultTTable(string ProductCalID, string StartDate, string EndDate, Repository<ProductCal> re)
        {
            string ResultTable = "<table class=\"table table-dark table-bordered\"><tr><th>检测时间</th><th>异常结果</th><th>异常来源</th><th>原因分析</th><th>改善措施</th><th>检测人</th><th>检测日期</th></tr>";
            if (CommonHelper.IsEmpty(StartDate))
            {
                StartDate = DateTime.Now.AddDays(-30).ToString();
            }
            if (CommonHelper.IsEmpty(EndDate))
            {
                EndDate = DateTime.Now.ToString();
            }
            string sql = @"select CONVERT(varchar(100), b.StartDate, 120)+'~'+CONVERT(varchar(100), b.EndDate, 8) as durdate,c.FaultContent,c.FaultFrom,
 c.CauseAnaly,c.ImproveMeasure,c.CreatrBy,cast( c.CreateDate as date)
from ProductCal a 
left join ProductCalDetail b on a.ProductCalID=b.ProductCalID
inner join CalFault c on b.ProductCalDID=c.ProductCalDID
where a.ProductCalID='{0}' and b.StartDate>='{1}' and b.StartDate<='{2}'
order by CONVERT(varchar(100), b.StartDate, 120)+'~'+CONVERT(varchar(100), b.EndDate, 8)";
            sql = string.Format(sql, ProductCalID, StartDate, EndDate);
            DataTable dt = re.FindTableBySql(sql);
            if (dt.Rows.Count > 0)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    ResultTable += "<tr>";
                    for (int j = 0; j < dt.Columns.Count; j++)
                    {
                        ResultTable += "<td>" + dt.Rows[i][j].ToString() + "</td>";
                    }
                    ResultTable += "</tr>";
                }
            }
            ResultTable += "</table>";
            return ResultTable;
        }

        /// <summary>
        /// 潜在风险拼接table
        /// </summary>
        /// <param name="ProductCalID"></param>
        /// <param name="StartDate"></param>
        /// <param name="EndDate"></param>
        /// <param name="re"></param>
        /// <returns></returns>
        public static string PotentialRisk(string ProductCalID, string StartDate, string EndDate, Repository<ProductCal> re)
        {
            if (CommonHelper.IsEmpty(StartDate))
            {
                StartDate = DateTime.Now.AddDays(-30).ToString();
            }
            if (CommonHelper.IsEmpty(EndDate))
            {
                EndDate = DateTime.Now.ToString();
            }
            string ResultTable = "<table class=\"table table-dark table-bordered\"><tr><th>开始日期</th><th>结束日期</th><th>潜在风险</th><th>失效后果</th><th>改善措施</th></tr>";
            string sql = @" select StartDate,EndDate,PotentialContent,FailureEffect,Action from PotentialRisk where 1=1 and DepartmentID in 
(select DepartmentID from ProductCal where ProductCalID='{0}') and ((StartDate<='{1}' and EndDate>='{1}' ) or (startdate>='{1}' and startdate<='{2}') )  ";
            sql = string.Format(sql, ProductCalID, StartDate, EndDate);
            DataTable dt = re.FindTableBySql(sql);
            if (dt.Rows.Count > 0)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    ResultTable += "<tr>";
                    for (int j = 0; j < dt.Columns.Count; j++)
                    {
                        ResultTable += "<td>" + dt.Rows[i][j].ToString() + "</td>";
                    }
                    ResultTable += "</tr>";
                }
            }
            return ResultTable;
        }
        /// <summary>
        /// 计算时间段内的CPK
        /// </summary>
        /// <param name="ProductCalID"></param>
        /// <param name="StartDate"></param>
        /// <param name="EndDate"></param>
        /// <param name="re"></param>
        /// <returns></returns>
        public static string CalCPK(string ProductCalID, string StartDate, string EndDate, Repository<ProductCal> re)
        {
            if (CommonHelper.IsEmpty(StartDate))
            {
                StartDate = DateTime.Now.AddDays(-30).ToString();
            }
            if (CommonHelper.IsEmpty(EndDate))
            {
                EndDate = DateTime.Now.ToString();
            }
            string sql = @" select  CONVERT(varchar(100), b.StartDate, 120)+'~'+CONVERT(varchar(100), b.EndDate, 8) as riqi,
AVG(InputValue) as xbar,MAX(InputValue)-MIN(InputValue) as r,
c.XLowLimit,c.XUpperLimit,c.XCenterLine,
c.RLowLimit,c.RUpperLimit,c.RCenterLine,
c.StandardLowLimit,c.StandardUpperLimit,c.StandardCenterLine,c.GroupNum
from ProductCalData a 
left join ProductCalDetail  b on a.ProductCalDID=b.ProductCalDID 
left join ProductCal c on b.ProductCalID=c.ProductCalID
where a.ProductCalID='{0}'  and cast(b.StartDate as date)>=cast('{1}' as date) and cast(b.StartDate as date)<=cast('{2}' as date) and  b.State='已完成'
group by a.ProductCalDID,b.StartDate,b.EndDate,c.XLowLimit,c.XUpperLimit,c.XCenterLine,c.RLowLimit,c.RUpperLimit,c.RCenterLine,
c.StandardLowLimit,c.StandardUpperLimit,c.StandardCenterLine,c.GroupNum  
order by StartDate ";
            sql = string.Format(sql, ProductCalID, StartDate, EndDate);
            DataTable dt = re.FindTableBySql(sql);
            if (dt.Rows.Count > 0)
            {
                decimal rAverage = (decimal)dt.Compute("Avg(" + "r" + ")", "true");
                decimal xbarAverage = (decimal)dt.Compute("Avg(" + "xbar" + ")", "true");

                decimal fenzi1 = Convert.ToDecimal(dt.Rows[0]["StandardUpperLimit"].ToString()) - xbarAverage;
                if (fenzi1 < 0)
                {
                    fenzi1 = fenzi1 * -1;
                }
                decimal fenzi2 = xbarAverage - Convert.ToDecimal(dt.Rows[0]["StandardUpperLimit"].ToString());
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
                decimal fenmu = 3 * rAverage / CpkCanshu(Convert.ToInt32(dt.Rows[0]["GroupNum"].ToString()));
                return (fenzi / fenmu).ToString("0.00");

                //var JsonData = new
                //{
                //    Success = true,
                //    Code = "1",
                //    rCenterLine=rCenterLine,
                //    rUCL=rUCL,
                //    xbarCenterLine=xbarCenterLine,
                //    xbarUCL=xbarUCL,
                //    xbarLCL=xbarLCL,
                //    Data = dt,
                //};


            }
            return "0";
        }
        /// <summary>
        /// 参数：
        ///M=2  1.13
        ///M=3  1.69
        ///M=4  2.06
        ///M=5  2.33
        ///M=6  2.53
        ///M=7  2.7
        ///M=8  2.85
        ///M=9  2.97
        ///M=10 3.08
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public static decimal CpkCanshu(int i)
        {
            switch (i)
            {
                case 1:
                    return 1.00M;
                case 2:
                    return 1.13M;
                case 3:
                    return 1.69M;
                case 4:
                    return 2.06M;
                case 5:
                    return 2.33M;
                case 6:
                    return 2.53M;
                case 7:
                    return 2.7M;
                case 8:
                    return 2.85M;
                case 9:
                    return 2.97M;
                case 10:
                    return 3.08M;
                default:
                    return 0M;
            }
        }


    }
}