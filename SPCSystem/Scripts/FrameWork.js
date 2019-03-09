/**
 * 
 *author by shenjun 2018.08.22
 */

function GetQuery(key) {
    
    var search = location.search.slice(1); //得到get方式提交的查询字符串

    var arr = search.split("&");
    for (var i = 0; i < arr.length; i++) {
        var ar = arr[i].split("=");
        if (ar[0] == key) {
            if (unescape(ar[1]) == 'undefined') {
                return "";
            } else {
                return unescape(ar[1]);
            }
        }
    }
    return "";
}


/*
自动获取页面控件值
*/
function GetWebControls(element) {
    var reVal = "";
    $(element).find('input,select,textarea').each(function (r) {
        var id = $(this).attr('id');
        var value = $(this).val();
        var type = $(this).attr('type');
        switch (type) {
            case "checkbox":
                if ($(this).attr("checked")) {
                    reVal += '"' + id + '"' + ':' + '"1",'
                } else {
                    reVal += '"' + id + '"' + ':' + '"0",'
                }
                break;
            default:
                //使用正则替换回车，有回车符号的话转json会报错
                value = value.replace(/[\n]/ig, '↵');
                if (value == "") {
                    value = "&nbsp;";
                }
                reVal += '"' + id + '"' + ':' + '"' + $.trim(value) + '",'
                break;
        }
    });
    reVal = reVal.substr(0, reVal.length - 1);
    return jQuery.parseJSON('{' + reVal + '}');
}
/*
自动给控件赋值
*/
function SetWebControls(data) {
    for (var key in data) {
        var id = $('#' + key);
        var value = $.trim(data[key]).replace("&nbsp;", "");
        var reg = new RegExp('↵', "g");
        var type = id.attr('type');
        switch (type) {
            case "checkbox":
                if (value == 1) {
                    id.attr("checked", 'checked');
                } else {
                    id.removeAttr("checked");
                }
                $('input').customInput();
                break;
            default:
                //将后台储存的回车符号转换回来
                value = value.replace(reg, '\r\n');
                id.val(value);
                break;
        }
    }
}
/*
自动给控件赋值、对Lable
*/
function SetWebLable(data) {
    for (var key in data) {
        var id = $('#' + key);
        var value = $.trim(data[key]).replace("&nbsp;", "");
        id.text(value);
    }
}

/* 
请求Ajax 带返回值
*/
function getAjax(url, postData, callBack) {
    //alert(RootPath() + url);
    $.ajax({
        type: 'post',
        dataType: "text",
        url: RootPath() + url,
        data: postData,
        cache: false,
        async: false,
        success: function (data) {
            callBack(data);
            //Loading(false);
        },
        error: function (data) {
            console.log(data);
            alert("error:" + JSON.stringify(data));
            //Loading(false);
        }
    });
}
function AjaxJson(url, postData, callBack) {
    $.ajax({
        url: RootPath() + url,
        type: "post",
        data: postData,
        dataType: "json",
        async: false,
        success: function (data) {
            if (data.Code == "-1") {
                //Loading(false);
                layer.msg(data.Message);
                //alertDialog(data.Message, -1);
            } else {
                //Loading(false);
                callBack(data);
            }
        },
        error: function (data) {
            //Loading(false);
            layer.msg(data.responseText);
            //alertDialog(data.responseText, -1);
        }
    });
}

//js获取网站根路径(站点及虚拟目录)
function RootPath() {
    var strFullPath = window.document.location.href;
    var strPath = window.document.location.pathname;
    var pos = strFullPath.indexOf(strPath);
    var prePath = strFullPath.substring(0, pos);
    var postPath = strPath.substring(0, strPath.substr(1).indexOf('/') + 1);
    //return (prePath + postPath);如果发布IIS，有虚假目录用用这句
    return (prePath);
}

/**
* 获取动态table：键、值，返回JSON
* var GetTableData = GetTableDataJson("table的ID")，一般多用于明细表，后台可直接转List;
*/
function GetTableDataJson(tableId) {
    var item_Key_Value = "";
    var index = 1;
    var trjson = "";
    if ($(tableId + " tbody tr").length > 0) {
        $(tableId + " tbody tr").each(function () {
            var tdjson = "";
            $(this).find('td').find('input,select,textarea').each(function () {
                var pk_id = $(this).attr('id');
                var pk_value = "";
                if ($("#" + pk_id).attr('type') == "checkbox") {
                    if ($("#" + pk_id).attr("checked")) {
                        pk_value = "1";
                    } else {
                        pk_value = "0";
                    }
                } else {
                    pk_value = $("#" + pk_id).val().replace(/\\n/g, "\\n")
                        .replace(/\\'/g, "\\'")
                        .replace(/\\"/g, "\\\"")
                        .replace(/\\&/g, "\\&")
                        .replace(/\\r/g, "\\r")
                        .replace(/\\t/g, "\\t")
                        .replace(/\\b/g, "\\b")
                        .replace(/\\f/g, "\\f");

                }
                var array = new Array();
                array = pk_id.split("➩"); //字符分割
                tdjson += '"' + array[0] + '"' + ':' + '"' + $.trim(pk_value) + '",'
            })
            tdjson = tdjson.substr(0, tdjson.length - 1);
            trjson += '{' + tdjson + '},';
        });
    } else {
        $(tableId + " tr").each(function () {
            var tdjson = "";
            $(this).find('td').find('input,select,textarea').each(function () {
                var pk_id = $(this).attr('id');
                var pk_value = "";
                if ($("#" + pk_id).attr('type') == "checkbox") {
                    if ($("#" + pk_id).attr("checked")) {
                        pk_value = "1";
                    } else {
                        pk_value = "0";
                    }
                } else {
                    pk_value = $("#" + pk_id).val();
                }
                var array = new Array();
                array = pk_id.split("➩"); //字符分割
                tdjson += '"' + array[0] + '"' + ':' + '"' + $.trim(pk_value) + '",'
            })
            tdjson = tdjson.substr(0, tdjson.length - 1);
            trjson += '{' + tdjson + '},';
        });
    }
    trjson = trjson.substr(0, trjson.length - 1);
    if (trjson == '{}') {
        trjson = "";
    }

    return '[' + trjson + ']';
}


function IsDelData(id) {
    var isOK = true;
    if (id == undefined || id == "" || id == 'null' || id == 'undefined') {
        isOK = false;
        layer.msg("您没有选中任何项, 请您选中后再操作。");
    }
    return isOK;
}
function IsChecked(id) {
    var isOK = true;
    if (id == undefined || id == "" || id == 'null' || id == 'undefined') {
        isOK = false;
        layer.msg("您没有选中任何项, 请您选中后再操作。");
    } else if (id.split(",").length > 1) {
        isOK = false;
        layer.msg("很抱歉,一次只能选择一条记录。");
       
    }
    return isOK;
}

function delConfig(url, parm, count) {
    if (count == undefined) {
        count = 1;
    }
    if (confirm("您确定删除吗?")) {
        window.setTimeout(function () {
            AjaxJson(url, parm, function (data) {
                layer.msg(data.Message);
                //tipDialog(data.Message, 3, data.Code);
                if (data.Code > 0) {
                    windowload();
                }
            });
        }, 200);
    }
    //confirmDialog("提示", "注：您确定要删除 " + count + " 笔记录？该操作无法复原！", function (r) {
    //    if (r) {
    //        Loading(true, "正在删除数据...");
            
    //    }
    //});
}
/*
layer对话框（带：确认按钮、取消按钮）
*/
function layerDialog(url, _title, _width, _height) {
    layer.open({
        type: 2,  //2表示ifrmae弹出层
        title: _title,
        maxmin: true,
        shadeClose: true, //点击遮罩关闭层
        area: [_width + "px", _height + "px"],
        content: url,
        btn: ['确认', '取消'],
        yes: function (index, layero) {
            //按钮【按钮一】的回调
            //layero.AcceptClick();
            var iframeWin = window[layero.find('iframe')[0]['name']]; //得到iframe页的窗口对象，执行iframe页的方法：iframeWin.method();
            //调用授权提交方法
            var flag = iframeWin.AcceptClick();
        },
        btn2: function (index, layero) {
            //按钮【按钮二】的回调
            //return false 开启该代码可禁止点击该按钮关闭
            layer.close(index);
        },
        end: function () {     //窗口关闭事件
            //窗口关闭时的操作
        }
    });
}

/*
中间加载对话窗
*/
function Loading(bool, text) {
    var ajaxbg = top.$("#loading_background,#loading");
    if (!!text) {
        top.$("#loading").css("left", (top.$('body').width() - top.$("#loading").width()) / 2);
        top.$("#loading span").html(text);
    } else {
        top.$("#loading").css("left", "42%");
        top.$("#loading span").html("正在拼了命为您加载…");
    }
    if (bool) {
        ajaxbg.show();
    } else {
        ajaxbg.hide();
    }
}

/*
警告消息
msg: 显示消息
type：类型 >1：成功，<1：失败，其他：警告
*/
//function alertDialog(msg, type) {
//    var msg = "<div class='ui_alert'>" + msg + "</div>"
//    var icon = "";
//    if (type >= 1) {
//        icon = "succ.png";
//    } else if (type == -1) {
//        icon = "fail.png";
//    } else {
//        icon = "i.png";
//    }
//    top.$.dialog({
//        id: "alertDialog",
//        icon: icon,
//        content: msg,
//        title: "提示",
//        ok: function () {
//            return true;
//        }
//    });
//}

//获取显示区域的长宽,根据滚动条的长度计算得出
function GetPageSize() {
    var xScroll, yScroll;
    if (window.innerHeight && window.scrollMaxY) {
        xScroll = document.body.scrollWidth;
        yScroll = window.innerHeight + window.scrollMaxY;
    } else if (document.body.scrollHeight > document.body.offsetHeight) {
        xScroll = document.body.scrollWidth;
        yScroll = document.body.scrollHeight;
    } else {
        xScroll = document.body.offsetWidth;
        yScroll = document.body.offsetHeight;
    }
    var windowWidth, windowHeight;
    if (self.innerHeight) {
        windowWidth = self.innerWidth;
        windowHeight = self.innerHeight;
    } else if (document.documentElement && document.documentElement.clientHeight) {
        windowWidth = document.documentElement.clientWidth;
        windowHeight = document.documentElement.clientHeight;
    } else if (document.body) {
        windowWidth = document.body.clientWidth;
        windowHeight = document.body.clientHeight;
    }
    if (yScroll < windowHeight) {
        pageHeight = windowHeight;
    } else {
        pageHeight = yScroll;
    }
    if (xScroll < windowWidth) {
        pageWidth = windowWidth;
    } else {
        pageWidth = xScroll;
    }
    arrayPageSize = new Array(pageWidth, pageHeight, windowWidth, windowHeight)
    return arrayPageSize;
}

///返回格式化datetime
function CusDateFormat(datetime) {
    return datetime.getFullYear() + '-' + (Number(datetime.getMonth()) + 1) + '-' + datetime.getDate()
}