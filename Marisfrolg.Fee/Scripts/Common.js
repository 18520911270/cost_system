//创建Socket连接

function SocketConnection(controllerName, pEmployeeNo, pShopNo,pConnectionString)
{
   this.employeeNo = pEmployeeNo;
   this.shopNo = pShopNo;
   this.connectionString = pConnectionString;//'ws://192.168.3.221:1818'
   
    var wsImpl = window.WebSocket || window.MozWebSocket;
    // create a new websocket and connect
    window.ws = new wsImpl(pConnectionString);
   
    // when data is comming from the server, this metod is called
    //ws.onmessage = function (evt) {
    //    alert(evt.data);
        
        
    //};

    // when the connection is established, this method is called
    ws.onopen = function () {
        ws.send(controllerName + "|" + pEmployeeNo + "|" + pShopNo);
    };

    // when the connection is closed, this method is called
    ws.onclose = function () {
       
    }

    this.socket = window.ws;
}

//验证输入值是否是正整数或小数且小数位为一位
function IsNumber(input)
{
    //var reg = /^\+?(\d*\.\d{1})$/;

    var reg = /^\d+(\.\d{1,2})?$/;
    if (!reg.test(input)) {
        return false;
    }
    return true;
}

function toDecimal(x) {
    var f = parseFloat(x);
    if (isNaN(f)) {
        return '0';
    }
    var f = Math.round(x * 100) / 100;
    var s = f.toString();
    var rs = s.indexOf('.');
    if (rs < 0)
    {
        rs = s.length;
        s += '.';
    }
    while (s.length <= rs + 2) {
        s += '0';
    }
    return s;
}

function toDecimalWithDot(x) {
    var f = parseFloat(x);
    if (isNaN(f)) {
        return false;
    }
    var f = Math.round(x * 100) / 100;
    var s = f.toString();
    var rs = s.indexOf('.');
    if (rs < 0) {
        rs = s.length;
        s += '.';
    }
    while (s.length <= rs + 1) {
        s += '0';
    }
    return s;
}

function InitGrid(columns, url, exportFileName, gridContainer, toolbarContainer, Index, drillDown)
{
    var gridOption = {
        lang: 'zh-cn',
        ajaxLoad: true,
        loadURL: url,
        exportFileName: exportFileName,
        columns: columns,
        gridContainer: gridContainer,
        toolbarContainer: toolbarContainer,
        tools: '',
        pageSize: 15,
        pageSizeLimit: [15, 20, 50],
        onCellMouseDown: function (value, record, column, grid, dataNo, columnNo, cell, row, extraCell, e) {
            if (columnNo == Index) {
                if (typeof (drillDown) == "function")
                {
                    drillDown();
                }
            }
        },
    };
   
    $("table[id*=dt_grid_]").remove();
    $.fn.DtGrid.init(gridOption).load();
}

//生成GUID
function Guid() {
    var s = [];
    var hexDigits = "0123456789abcdef";
    for (var i = 0; i < 36; i++) {
        s[i] = hexDigits.substr(Math.floor(Math.random() * 0x10), 1);
    }
    s[14] = "4";
    s[19] = hexDigits.substr((s[19] & 0x3) | 0x8, 1);
    s[8] = s[13] = s[18] = s[23] = "-";
    var uuid = s.join("");
    return uuid;
}

//日期格式转换
function toDate(value) {
  
    if (value == null)
        return "";

    if (value.indexOf("/Date(") < 0)
        return value;

    var date = new Date(parseInt(value.replace("/Date(", "").replace(")/", ""), 10));
    var month = date.getMonth() + 1 < 10 ? "0" + (date.getMonth() + 1) : date.getMonth() + 1;
    //将日期格式化为 yyyy-MM-dd
    var day = date.getDate() < 10 ? "0" + date.getDate() : date.getDate();


    return date.getFullYear() + "-" + month + "-" + day;
}

Date.prototype.format = function (format) {
    var o = {
        "M+": this.getMonth() + 1, //month
        "d+": this.getDate(), //day
        "h+": this.getHours(), //hour
        "m+": this.getMinutes(), //minute
        "s+": this.getSeconds(), //second
        "q+": Math.floor((this.getMonth() + 3) / 3), //quarter
        "S": this.getMilliseconds() //millisecond
    }

    if (/(y+)/.test(format))
        format = format.replace(RegExp.$1, (this.getFullYear() + "").substr(4 - RegExp.$1.length));

    for (var k in o)
    {
        if (new RegExp("(" + k + ")").test(format))
        {
            format = format.replace(RegExp.$1, RegExp.$1.length == 1 ? o[k] : ("00" + o[k]).substr(("" + o[k]).length));
        }
    }
    
    return format;
}


//js基础组件
if (typeof $JsSupportComponent != "function") {
    var $JsSupportComponent = new function () {

        //年月类
        var YearMonth = function (year, month) {
            this.year = year;
            this.month = month;
        }

        //年月日类
        var YearMonthDay = function (year, month, day) {
            this.year = year;
            this.month = month;
            this.day = day;
        }

        //年季
        var YearSeason = function (year, season) {
            this.year = year;
            this.season = season;
        }

        this.Season = {
            /*
                           分离 XX年XX季 格式
                           eg:2015年春季
                        */
            SplitYearSeason1: function (yearSeason) {
                var obj = new YearSeason();
                var splitArray = yearSeason.split("年");
                obj.year = splitArray[0];
                splitArray = splitArray[1].split("季");
                obj.season = $JsSupportComponent.Season.GetYearSeasonCode(splitArray[0]);
                return obj;
            },
            GetYearSeasonCode: function (seasonName) {
                var res = "";
                switch (seasonName) {
                    case "春":
                        res = "0001";
                        break;
                    case "夏":
                        res = "0002";
                        break;
                    case "秋":
                        res = "0003";
                        break;
                    case "冬":
                        res = "0004";
                        break;
                    default:
                        break;
                }
                return res;
            },
            GetYearSeasonName: function (seasonCode) {
                var res = "";
                switch (seasonCode) {
                    case "0001":
                        res = "春"
                        break;
                    case "0002":
                        res = "夏"
                        break;
                    case "0003":
                        res = "秋"
                        break;
                    case "0004":
                        res = "冬"
                        break;
                    default:
                        break;
                }
                return res;
            },
        };

        /*
        日期类操作命名空间
        */
        this.Date = {
            /*
                分离XXXX年格式
                eg:2015年
            */
            SplitYear: function (year) {
                var obj = "";
                var splitArray = year.split("年");
                obj = splitArray[0];
                return obj;
            },
            /*
                分离XXXX年XX月格式
                eg:2015年12月
            */
            SplitYearMonth1: function (yearMonth) {
                var obj = new YearMonth();
                var splitArray = yearMonth.split("年");
                obj.year = splitArray[0];
                splitArray = splitArray[1].split("月");
                obj.month = splitArray[0];
                return obj;
            },
            /*
                分离 XXXX/XX/XX 格式
                eg:2015/5/6
            */
            SplitYearMonthDay1: function (yearMonthDay) {
                var obj = new YearMonthDay();
                var splitArray = yearMonthDay.split("/");
                obj.year = splitArray[0];
                obj.month = splitArray[1];
                obj.day = splitArray[2];
                return obj;
            },
            /*
                分离XXXX年XX月XX日
            */
            SplitYearMonthDay2: function (yearMonthDay) {
                var obj = new YearMonthDay();
                var splitArray = yearMonthDay.split("年");
                obj.year = splitArray[0];
                splitArray = splitArray[1].split("月");
                obj.month = splitArray[0];
                splitArray = splitArray[1].split("日");
                obj.day = splitArray[0];
                return obj;
            },
            
        };
    };
}

