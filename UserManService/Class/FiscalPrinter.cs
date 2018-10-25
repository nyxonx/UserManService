using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;

namespace UserManService
{
    public enum FPTextAlign
    {
        Center,
        Right,
        Left,
    }
    public enum FPPaymentType
    {
        Cash,
        Card,
        Cheque,
        Coupon,
        ForeignCurrency,
    }
    public enum FPLibError
    {
        SUCCESS = 0,
        UNDEFINED = 256,
        BAD_INPUT_DATA = 257,
        TIMEOUT = 258,
        NACK = 259,
        CRC = 260,
        BAD_RECEIPT = 261,
        BAD_RESPONSE = 262,
        RETRIED = 263,
        NO_PRINTER = 265,
        PRINTER_BUSY = 266,
        NBL_NOT_SAME = 267,
        BUSY_TIMEOUT = 269,
        UNKNOWN_DEVICE = 270,
        PORT_NOT_OPEN = 271,
        PAPER_EMPTY = 272,
    }
    public enum FPChangeType
    {
        Cash,
        SameAsPayment,
        ForeignCurrency,
    }
    public class FPException : Exception
    {
        public static string lang = "en";
        public static FPLibError last_error;
        private Exception base_exception;
        public int ErrorCode;

        public bool IsFPError
        {
            get
            {
                if (this.ErrorCode > 0)
                    return this.ErrorCode < 256;
                return false;
            }
        }

        public override string Message
        {
            get
            {
                if (FPException.lang == "bg")
                {
                    if (this.ErrorCode == 0)
                        return "Операцията завърши успешно.";
                    if (this.ErrorCode < 256)
                        return string.Format("ФП: {0}; Команда: {1}.", (object)new string[16]
            {
              "OK",
              "Няма хартия",
              "Препълване в общите регистри",
              "Несверен / грешен часовник",
              "Отворен фискален бон",
              "Сметка с остатък за плащане (отворен бон)",
              "Отворен нефискален бон",
              "Сметка с приключено плащане (отворен бон)",
              "Фискална памет в режим само за четене",
              "Грешна парола или непозволена команда",
              "Липсващ външен дисплей",
              "24 часа без дневен отчет (блокировка)",
              "Прегрят принтер",
              "Спад на напрежение във фискален бон",
              "Препълване в електронната контролна лента",
              "Недостатъчни условия"
            }[this.ErrorCode >> 4], (object)new string[16]
            {
              "OK",
              "Невалидна",
              "Непозволена",
              "Непозволена поради ненулев отчет",
              "Синтактична грешка",
              "Синтактична грешка / препълване на входните регистри",
              "Синтактична грешка / нулев входен регистър",
              "Липсва транзакция, която да се войдира",
              "Недостатъчна налична сума",
              "Конфликт в данъчните групи",
              "?",
              "?",
              "?",
              "?",
              "?",
              "?"
            }[this.ErrorCode & 15]);
                    switch (this.ErrorCode)
                    {
                        case 257:
                            return "Некоректни входни данни.";
                        case 258:
                            return "Просрочено време за отговор от фискалния принтер.";
                        case 259:
                            return "Отрицателен (NACK) отговор от фискалния принтер.";
                        case 260:
                            return "Грешна контролна сума.";
                        case 261:
                            return "Получен е грешен отговор от фискалния принтер; Грешка при комуникация.";
                        case 262:
                            return "Получени са грешни данни от фискалния принтер; Грешка при комуникация.";
                        case 263:
                            return "Фискалният принтер не може да изпълни операцията; Опитайте по-късно.";
                        case 265:
                            return "Фискалният принтер не може да бъде открит.";
                        case 266:
                            return "Фискалният принтер е зает; Опитайте по-късно.";
                        case 267:
                            return "Различен номер на блок на данните; Грешка при комуникация.";
                        case 269:
                            return "Фискалният принтер е зает; Просрочено време за отговор.";
                        case 270:
                            return "Непознато устройство.";
                        case 271:
                            return "Серийният порт не е отворен.";
                        case 272:
                            return "Няма хартия.";
                        default:
                            return "Непозната грешка.";
                    }
                }
                else
                {
                    if (this.ErrorCode == 0)
                        return "Operation completed successfully.";
                    if (this.ErrorCode < 256)
                        return string.Format("FP: {0}; Command: {1}.", (object)new string[16]
            {
              "OK",
              "Paper out",
              "Daily registers overflow",
              "Invalid RTC date/time",
              "Open fiscal receipt",
              "Bill remainder not paid; Open receipt",
              "Open non-fiscal receipt",
              "Bill payment finished; Open receipt",
              "Fiscal memory is read-only",
              "Wrong password or command not allowed",
              "Missing display",
              "24 hours without daily report",
              "Printer overheat",
              "Power down",
              "Electronic journal is full",
              "Not enough conditions met"
            }[this.ErrorCode >> 4], (object)new string[16]
            {
              "OK",
              "Invalid",
              "Illegal",
              "Denied because of uncommited report",
              "Syntax error",
              "Syntax error / Input register overflow",
              "Syntax error / Input register is zero",
              "Missing transaction for void",
              "Insufficient subtotal",
              "Tax groups conflict",
              "?",
              "?",
              "?",
              "?",
              "?",
              "?"
            }[this.ErrorCode & 15]);
                    switch (this.ErrorCode)
                    {
                        case 257:
                            return "Incorrect input data.";
                        case 258:
                            return "Timeout while waiting for fiscal printer response.";
                        case 259:
                            return "Negative response from FP.";
                        case 260:
                            return "CRC error.";
                        case 261:
                            return "Wrong FP receipt; Communication error.";
                        case 262:
                            return "Wrong FP response content; Communication error.";
                        case 263:
                            return "The Fiscal Printer cannot complete the operation; Try again later.";
                        case 265:
                            return "Fiscal Printer device cannot be found.";
                        case 266:
                            return "The Fiscal Printer is busy; Try again later.";
                        case 267:
                            return "Wrong data block number; Communication error.";
                        case 269:
                            return "The Fiscal Printer is busy more than expected.";
                        case 270:
                            return "Incompatible device; Not a Fiscal Printer.";
                        case 271:
                            return "The serial port is not open.";
                        case 272:
                            return "Out of paper or open cover.";
                        default:
                            return "Unknown error.";
                    }
                }
            }
        }

        public FPException(int error)
        {
            this.ErrorCode = error;
            FPException.last_error = (FPLibError)error;
            this.base_exception = (Exception)null;
        }

        public FPException(FPLibError error)
        {
            this.ErrorCode = (int)error;
            FPException.last_error = error;
            this.base_exception = (Exception)null;
        }

        public FPException(int error, Exception innerException)
        {
            this.ErrorCode = error;
            FPException.last_error = (FPLibError)error;
            this.base_exception = innerException;
        }

        public FPException(FPLibError error, Exception innerException)
        {
            this.ErrorCode = (int)error;
            FPException.last_error = error;
            this.base_exception = innerException;
        }

        public override Exception GetBaseException()
        {
            return this.base_exception;
        }
    }


    public class FP : IDisposable
    {
        public bool WaitAfterSlowCommands = true;
        public int TextLineWidth = 38;
        public int GSEndByte = -1;
        private int rw_timeout = 3000;
        private int gs_timeout = 300;
        private int ping_timeout = 300;
        private bool? new_ping = new bool?();
        private const int PayTypesCount = 4;
        public const byte STX = 2;
        public const byte ETX = 10;
        public const byte ANTIECHO = 3;
        public const byte PING = 4;
        public const byte BUSY = 5;
        public const byte ACK = 6;
        public const byte OUTOFPAPER = 7;
        public const byte PING_NEW = 9;
        public const byte RETRY = 14;
        public const byte NACK = 21;
        private const int ping_retries = 10;
        private const int cmd_retries = 3;
        public const int busy_timeout = 20000;
        public const double LibraryVersion = 3.6;
        private string _country;
        public Encoding TextEncoding;
        private static int DeviceParamsCount;
        private static int TaxGroupCount;
        public char TaxGroupBase;
        public byte[] last_response_raw;
        private static byte next_cmd_id;
        private byte cmd_id;
        public SerialPort port;

        public string Country
        {
            get
            {
                return this._country;
            }
            set
            {
                this._country = value.ToLower();
                switch (this._country)
                {
                    case "bg":
                        this.TextEncoding = Encoding.GetEncoding(1251);
                        FP.TaxGroupCount = 8;
                        FP.DeviceParamsCount = 8;
                        this.TaxGroupBase = '0';
                        FPException.lang = "bg";
                        break;
                    case "gr":
                        this.TextEncoding = Encoding.GetEncoding(1253);
                        FP.TaxGroupCount = 5;
                        FP.DeviceParamsCount = 8;
                        this.TaxGroupBase = '1';
                        FPException.lang = "en";
                        break;
                    case "ke":
                        this.TextEncoding = Encoding.GetEncoding(1252);
                        FP.TaxGroupCount = 5;
                        FP.DeviceParamsCount = 9;
                        this.TaxGroupBase = '1';
                        FPException.lang = "en";
                        break;
                    default:
                        throw new FPException(FPLibError.BAD_INPUT_DATA);
                }
            }
        }

        public bool IsPrinterBusy
        {
            get
            {
                return this.Ping((byte)5, FPLibError.PRINTER_BUSY, 10, false) == FPLibError.PRINTER_BUSY;
            }
        }

        public static event FP.LogEventHandler PortLog = null;

        public FP()
        {
            this.Country = "bg";
        }

        public FP.Status GetStatus()
        {
            return new FP.Status(this.SendCommand(true, " ", new object[0]));
        }

        public string GetVersion()
        {
            return this.SendCommandGetString("!");
        }

        public void Diagnostic()
        {
            this.SendCommand(true, "\"", new object[0]);
            this.WaitSlowCommand();
        }

        public void ClearDisplay()
        {
            this.SendCommand(1 != 0, "{0}", (object)'$');
        }

        public void DisplayLine1(string text)
        {
            this.SendCommand(1 != 0, "%{0}", (object)FP.fix_len(text, 20));
        }

        public void DisplayLine2(string text)
        {
            this.SendCommand(1 != 0, "&{0}", (object)FP.fix_len(text, 20));
        }

        public void Display(string text)
        {
            this.SendCommand(1 != 0, "{0}{1}", (object)'\'', (object)FP.fix_len(text, 40));
        }

        public void DisplayDateTime()
        {
            this.SendCommand(true, "(", new object[0]);
        }

        public void OpenCashDrawer()
        {
            this.SendCommand(true, "*", new object[0]);
        }

        public void PaperFeed()
        {
            this.SendCommand(true, "+", new object[0]);
        }

        public void PaperCut()
        {
            this.SendCommand(true, ")", new object[0]);
        }

        public void SetSerialNumber(string password, string serialNum)
        {
            if (password.Length != 6 || serialNum.Length != 9)
                throw new FPException(FPLibError.BAD_INPUT_DATA);
            this.SendCommand(1 != 0, "{0}{1};{2}", (object)'@', (object)password, (object)serialNum);
        }

        public void SetTaxNumber(string password, string taxNum, string fiscalNum)
        {
            if (password.Length != 6 || taxNum.Length != 15 || fiscalNum.Length != 12)
                throw new FPException(FPLibError.BAD_INPUT_DATA);
            this.SendCommand(1 != 0, "{0}{1};1;{2};{3}", (object)'A', (object)password, (object)taxNum, (object)fiscalNum);
        }

        public void Fiscalize(string password)
        {
            if (password.Length != 6)
                throw new FPException(FPLibError.BAD_INPUT_DATA);
            this.SendCommand(1 != 0, "{0}{1};2", (object)'A', (object)password);
        }

        public void SetTaxPecents(string password, Decimal[] taxRates)
        {
            if (password.Length != 6 || taxRates.Length != FP.TaxGroupCount)
                throw new FPException(FPLibError.BAD_INPUT_DATA);
            string format = string.Format("{0}{1}", (object)'B', (object)password);
            foreach (Decimal taxRate in taxRates)
                format += string.Format(";{0}%", (object)FP.d2s(taxRate, 2));
            this.SendCommand(true, format, new object[0]);
        }

        public void SetDecimalPoint(string password, bool useFractions)
        {
            if (password.Length != 6)
                throw new FPException(FPLibError.BAD_INPUT_DATA);
            this.SendCommand(1 != 0, "{0}{1}:{2}", (object)'C', (object)password, (object)FP.Flag(useFractions, '2', '0'));
        }

        public void UpdateHeader(string password, char comCode)
        {
            if (password.Length != 6 || (int)comCode < 48 || (int)comCode > 51)
                throw new FPException(FPLibError.BAD_INPUT_DATA);
            this.SendCommand(1 != 0, "{0}{1};{2}", (object)'W', (object)comCode, (object)password);
        }

        public void SetPayType(FPPaymentType payType, string name)
        {
            if (payType == FPPaymentType.ForeignCurrency)
                throw new FPException(FPLibError.BAD_INPUT_DATA);
            this.SendCommand(1 != 0, "{0}{1};{2}", (object)'D', (object)payType, (object)FP.fix_len(name, 10));
        }

        public void SetCurrency(string name, Decimal rate)
        {
            this.SendCommand(1 != 0, "{0}{1};{2};{3}", (object)'D', (object)4, (object)FP.fix_len(name, 10), (object)rate.ToString("0000.00000", (IFormatProvider)CultureInfo.InvariantCulture));
        }

        public void SetParameters(FP.Parameters pas)
        {
            this.SendCommand(1 != 0, "{0}{1};{2};{3};{4};{5};{6};{7};{8};{9};{10}", (object)'E', (object)pas.POSNumber.ToString("000#"), (object)FP.OzFlag(pas.PrintLogo), (object)FP.OzFlag(pas.OpenCashDrawer), (object)FP.OzFlag(pas.AutoCut), (object)FP.OzFlag(pas.TransparentDisplay), (object)FP.OzFlag(pas.ShortEJ), (object)FP.OzFlag(pas.TotalInForeignCurrency), (object)FP.OzFlag(pas.SmallFontEJ), (object)FP.OzFlag(pas.FreeTextInEJ), (object)FP.OzFlag(pas.SingleOperator));
        }

        public void SetDateTime(DateTime dt)
        {
            this.SendCommand(1 != 0, "{0}{1}", (object)'H', (object)dt.ToString("dd-MM-yy HH:mm:ss"));
            this.WaitSlowCommand();
        }

        public void SetHeaderLine(char index, string text)
        {
            if ((int)index != 58 && (int)index < 48 && (int)index > 57)
                throw new FPException(FPLibError.BAD_INPUT_DATA);
            string str;
            switch (index)
            {
                case '0':
                    str = FP.fix_len(text, 20);
                    break;
                case '9':
                    str = FP.fix_len(text, 14);
                    break;
                default:
                    str = FP.fix_len(text, 48);
                    break;
            }
            this.SendCommand(1 != 0, "{0}{1};{2}", (object)'I', (object)index, (object)str);
        }

        public void SetOperatorInfo(FP.OperatorInfo opInfo)
        {
            string str = FP.fix_len(opInfo.Password, 4);
            this.SendCommand(1 != 0, "{0}", (object)(string.Format("{0}{1};{2}", (object)'J', (object)opInfo.Number, (object)FP.fix_len(opInfo.Name, 20)) + ";" + str));
        }

        public void SetArticleInfo(FP.ArticleInfo ai)
        {
            if (ai.Number > 99999 || ai.Number < 0)
                throw new FPException(FPLibError.BAD_INPUT_DATA);
            string str = "";
            if (this.Country == "bg")
                str = ";0";
            byte[] bytes = this.TextEncoding.GetBytes(string.Format("{0}{1:00000};{2};{3};{4}{5}", (object)'K', (object)ai.Number, (object)FP.fix_len(ai.Name, 20), (object)FP.d2s(ai.Price), (object)ai.TaxGroup, (object)str));
            if (str.Length > 0)
                bytes[bytes.Length - 1] = (byte)(ai.Subgroup + 128);
            this.SendCommand(true, bytes);
        }

        public void SetSubgroupInfo(int num, string name, char tax_group)
        {
            this.SendCommand(1 != 0, "G{0:00};{1};{2}", (object)num, (object)FP.fix_len(name, 20), (object)tax_group);
        }

        public void SetLogoFile(int logoIndex, byte[] data)
        {
            byte[] numArray;
            if (logoIndex == -1)
                numArray = new byte[4]
        {
          (byte) 2,
          (byte) 57,
          (byte) 55,
          (byte) 76
        };
            else
                numArray = new byte[5]
        {
          (byte) 2,
          (byte) 57,
          (byte) 55,
          (byte) 77,
          (byte) (48 + logoIndex)
        };
            byte[] cmd = new byte[numArray.Length + data.Length];
            Array.Copy((Array)numArray, 0, (Array)cmd, 0, numArray.Length);
            Array.Copy((Array)data, 0, (Array)cmd, numArray.Length, data.Length);
            this.port.DiscardInBuffer();
            this.port.Write(numArray, 0, numArray.Length);
            this.log_output(numArray);
            this.port.Write(data, 0, data.Length);
            this.log_output(data);
            Thread.Sleep(1000);
            this.GetResponse(cmd);
        }

        public string GetFactoryNumber()
        {
            string[] array = this.SendCommandGetArray("{0}", (object)'`');
            if (array.Length < 1)
                throw new FPException(FPLibError.BAD_RESPONSE);
            return array[0];
        }

        public string GetFiscalNumber()
        {
            string[] array = this.SendCommandGetArray("{0}", (object)'`');
            if (array.Length < 2)
                throw new FPException(FPLibError.BAD_RESPONSE);
            return array[1];
        }

        public string GetTaxNumber()
        {
            return this.SendCommandGetString("{0}", (object)'a');
        }

        public Decimal[] GetTaxGroups()
        {
            string[] array = this.SendCommandGetArray("b");
            if (array.Length != FP.TaxGroupCount)
                throw new FPException(FPLibError.BAD_RESPONSE);
            Decimal[] numArray = new Decimal[array.Length];
            int num = 0;
            foreach (string str in array)
                numArray[num++] = Decimal.Parse(str.TrimEnd('%'), (IFormatProvider)CultureInfo.InvariantCulture);
            return numArray;
        }

        public int GetDecimalPoint()
        {
            string s = this.SendCommandGetString("c");
            if (s.Length != 1 || !char.IsDigit(s[0]))
                throw new FPException(FPLibError.BAD_RESPONSE);
            return int.Parse(s);
        }

        public FP.PayTypes GetPayTypes()
        {
            return new FP.PayTypes(this.SendCommandGetArray("d"));
        }

        public FP.Parameters GetParameters()
        {
            return new FP.Parameters(this.SendCommandGetArray("e"));
        }

        public DateTime GetDateTime()
        {
            return FP.s2dt(this.SendCommandGetString("h"));
        }

        public string GetHeaderLine(char index)
        {
            string str = this.SendCommandGetString("i{0}", (object)index);
            if (str.Length < 16 || (int)str[0] != (int)index)
                throw new FPException(FPLibError.BAD_RESPONSE);
            return str.Substring(1).Trim();
        }

        public FP.OperatorInfo GetOperatorInfo(int opNum)
        {
            FP.CheckOperatorNumber(opNum);
            return new FP.OperatorInfo(this.SendCommandGetArray(new int[3] { -1, 20, 4 }, "j{0}", (object)opNum));
        }

        public void PrintLogo()
        {
            this.SendCommand(true, "l", new object[0]);
        }

        public void PrintLogo(int logoNum)
        {
            if (logoNum < 0 || logoNum > 9)
                throw new FPException(FPLibError.BAD_INPUT_DATA);
            this.SendCommand(1 != 0, "l{0}", (object)logoNum);
            this.WaitSlowCommand();
        }

        public FP.LogosInfo GetLogosInfo()
        {
            return new FP.LogosInfo(this.SendCommandGetArray("{0}?", (object)'#'));
        }

        public void OpenNonFiscalReceipt(int opNum, string password, bool defer_print)
        {
            FP.CheckOperatorNumber(opNum);
            this.SendCommand(1 != 0, ".{0};{1};0" + (defer_print ? ";1" : ""), (object)opNum, (object)FP.fix_len(password, 4));
            this.WaitSlowCommand();
        }

        public void CloseNonFiscalReceipt()
        {
            this.SendCommand(true, "/", new object[0]);
            this.WaitSlowCommand();
        }

        public void OpenFiscalReceipt(int opNum, string password, bool detailed, bool show_vat, bool defer_print)
        {
            FP.CheckOperatorNumber(opNum);
            this.SendCommand(1 != 0, "0{0};{1};{2};{3};{4}", (object)opNum, (object)FP.fix_len(password, 4), (object)FP.OzFlag(detailed), (object)FP.OzFlag(show_vat), (object)FP.Flag(defer_print, '2', '0'));
            this.WaitSlowCommand();
        }

        public void SellItem(string name, char tax_group, Decimal price, Decimal quantity, Decimal discount, bool discount_in_percent)
        {
            if (price < new Decimal(-99999999) || price > new Decimal(99999999) || (quantity > new Decimal(999999999, 0, 0, false, (byte)3) || quantity < new Decimal(999999999, 0, 0, true, (byte)3)) || discount_in_percent && (discount < new Decimal(-999) || discount > new Decimal(999)))
                throw new FPException(FPLibError.BAD_INPUT_DATA);
            int len = 36;
            string format = string.Format("1{0};{1};{2}", (object)FP.fix_len(name, len), (object)tax_group, (object)FP.d2s(price));
            if (quantity != new Decimal(1))
                format = format + "*" + FP.d2s(quantity, 3);
            if (discount != new Decimal(0))
                format = !discount_in_percent ? format + ":" + FP.d2s(discount) : format + "," + FP.d2s(discount) + "%";
            this.SendCommand(true, format, new object[0]);
            this.WaitSlowCommand();
        }

        public void SellItemCitizen(string ticketNum, bool travelTicket, int paytype, Decimal price, Decimal quantity, Decimal discount)
        {
            if (price < new Decimal(-99999999) || price > new Decimal(99999999) || (quantity > new Decimal(999999999, 0, 0, false, (byte)3) || quantity < new Decimal(999999999, 0, 0, true, (byte)3)) || (discount < new Decimal(10000, 0, 0, true, (byte)2) || discount > new Decimal(9999, 0, 0, false, (byte)2)))
                throw new FPException(FPLibError.BAD_INPUT_DATA);
            string format = string.Format("={0};{1};{2};{3}", (object)ticketNum, travelTicket ? (object)"1" : (object)"0", (object)paytype, (object)FP.d2s(price));
            if (quantity != new Decimal(1))
                format = format + "*" + FP.d2s(quantity, 3);
            if (discount != new Decimal(0))
                format = format + "," + FP.d2s(discount) + "%";
            this.SendCommand(true, format, new object[0]);
            this.WaitSlowCommand();
        }

        public void SellItemDB(bool correction, int articleNum, Decimal quantity, Decimal discount, bool discount_in_percent)
        {
            if (articleNum < 0 || articleNum > 99999 || (quantity > new Decimal(999999999, 0, 0, false, (byte)3) || quantity < new Decimal(999999999, 0, 0, true, (byte)3)) || discount_in_percent && (discount < new Decimal(9999, 0, 0, true, (byte)2) || discount > new Decimal(9999, 0, 0, false, (byte)2)))
                throw new FPException(FPLibError.BAD_INPUT_DATA);
            string format = string.Format("{0}{1}{2}", (object)'2', (object)FP.Flag(correction, '-', '+'), (object)articleNum.ToString().PadLeft(5, '0'));
            if (quantity != new Decimal(1))
                format = format + "*" + FP.d2s(quantity, 3);
            if (discount != new Decimal(0))
                format = !discount_in_percent ? format + ":" + FP.d2s(discount) : format + "," + FP.d2s(discount) + "%";
            this.SendCommand(true, format, new object[0]);
            this.WaitSlowCommand();
        }

        public void SellItemDB(bool correction, int articleNum, Decimal price, Decimal quantity, Decimal discount, bool discount_in_percent)
        {
            if (articleNum < 0 || articleNum > 99999 || (price < new Decimal(-99999999) || price > new Decimal(99999999)) || (quantity > new Decimal(999999999, 0, 0, false, (byte)3) || quantity < new Decimal(999999999, 0, 0, true, (byte)3)) || discount_in_percent && (discount < new Decimal(9999, 0, 0, true, (byte)2) || discount > new Decimal(9999, 0, 0, false, (byte)2)))
                throw new FPException(FPLibError.BAD_INPUT_DATA);
            string format = string.Format("{0}{1}{2}${3}", (object)'2', (object)FP.Flag(correction, '-', '+'), (object)articleNum.ToString().PadLeft(5, '0'), (object)FP.d2s(price, 2));
            if (quantity != new Decimal(1))
                format = format + "*" + FP.d2s(quantity, 3);
            if (discount != new Decimal(0))
                format = !discount_in_percent ? format + ":" + FP.d2s(discount) : format + "," + FP.d2s(discount) + "%";
            this.SendCommand(true, format, new object[0]);
            this.WaitSlowCommand();
        }

        public Decimal Subtotal(Decimal discount, bool inPercents, bool print, bool onDisplay)
        {
            string format = string.Format("3{0};{1}", (object)FP.OzFlag(print), (object)FP.OzFlag(onDisplay));
            if (discount != new Decimal(0))
            {
                if (inPercents)
                {
                    if (discount < new Decimal(9999, 0, 0, true, (byte)2) || discount > new Decimal(9999, 0, 0, false, (byte)2))
                        throw new FPException(FPLibError.BAD_INPUT_DATA);
                    format = format + "," + FP.d2s(discount) + "%";
                }
                else
                {
                    if (discount < new Decimal(-999999999) || discount > new Decimal(9999999999L))
                        throw new FPException(FPLibError.BAD_INPUT_DATA);
                    format = format + ";" + FP.d2s(discount);
                }
            }
            string str = this.SendCommandGetString(format);
            try
            {
                return FP.s2d(str);
            }
            catch
            {
            }
            throw new FPException(FPLibError.BAD_RESPONSE);
        }

        public void Payment(FPPaymentType type, Decimal amount, bool no_change, FPChangeType change_type)
        {
            if (amount != new Decimal(-1) && (amount < new Decimal(0) || amount > new Decimal(9999999999L)))
                throw new FPException(FPLibError.BAD_INPUT_DATA);
            this.SendCommand(1 != 0, "5{0};{1};{2};{3}", (object)(byte)type, (object)(no_change ? 1 : 0), amount == new Decimal(-1) ? (object)"\"" : (object)FP.d2s(amount), (object)(byte)change_type);
            this.WaitSlowCommand();
        }

        public void CloseReceiptInCash()
        {
            this.SendCommand(true, "6", new object[0]);
            this.WaitSlowCommand();
        }

        public void PrintText(string text, FPTextAlign align)
        {
            string text1 = string.Empty;
            if (align == FPTextAlign.Center)
            {
                int totalWidth = text.Length + (this.TextLineWidth - text.Length) / 2;
                text1 = text.PadLeft(totalWidth, ' ');
            }
            else if (align == FPTextAlign.Right)
                text1 = text.PadLeft(this.TextLineWidth, ' ');
            else if (align == FPTextAlign.Left)
                text1 = text;
            this.PrintText(text1);
        }

        public void PrintText(string text)
        {
            this.SendCommand(1 != 0, "7{0}", (object)FP.fix_len(text, this.TextLineWidth));
            this.WaitSlowCommand();
        }

        public void CloseFiscalReceipt()
        {
            this.SendCommand(true, "8", new object[0]);
            this.WaitSlowCommand();
        }

        public void PrintDuplicate()
        {
            this.SendCommand(true, ":", new object[0]);
            this.WaitSlowCommand();
        }

        public void OfficialSums(int opNum, string pwd, char payType, Decimal amount, string expl)
        {
            FP.CheckOperatorNumber(opNum);
            this.SendCommand(1 != 0, ";{0};{1};{2};{3};@{4}", (object)opNum, (object)FP.fix_len(pwd, 4), (object)payType, (object)FP.d2s(amount), (object)FP.fix_len(expl, 34));
        }

        public void CancelFiscalReceipt()
        {
            this.SendCommand(true, "9", new object[0]);
            this.WaitSlowCommand();
        }

        public void TerminateReceipt(bool FinishPayment)
        {
            FP.Status status = this.GetStatus();
            if (status.OpenNonFiscalReceipt)
            {
                this.CloseNonFiscalReceipt();
            }
            else
            {
                if (!status.OpenFiscalReceipt)
                    return;
                Decimal amount = new Decimal(0);
                if (FinishPayment)
                {
                    try
                    {
                        this.CloseReceiptInCash();
                        return;
                    }
                    catch
                    {
                    }
                    amount = this.Subtotal(new Decimal(0), true, false, false);
                }
                else
                {
                    try
                    {
                        this.CancelFiscalReceipt();
                        return;
                    }
                    catch
                    {
                    }
                    FP.FiscalReceiptInfo fiscalReceiptInfo = this.GetFiscalReceiptInfo();
                    if (!fiscalReceiptInfo.IsOpen)
                        return;
                    Decimal[] taxGroupSubtotal = fiscalReceiptInfo.TaxGroupSubtotal;
                    for (int index = 0; index < taxGroupSubtotal.Length; ++index)
                    {
                        if (taxGroupSubtotal[index] != new Decimal(0))
                            this.SellItem("[void]", (char)((uint)this.TaxGroupBase + (uint)index), -taxGroupSubtotal[index], new Decimal(1), new Decimal(0), false);
                    }
                }
                if (amount == new Decimal(0))
                    amount = new Decimal(1, 0, 0, false, (byte)2);
                this.Payment(FPPaymentType.Cash, amount, true, FPChangeType.SameAsPayment);
                this.CloseFiscalReceipt();
            }
        }

        public FP.ArticleInfo GetArticleInfo(int num)
        {
            string[] array = this.SendCommandGetArray(new int[2] { -1, 20 }, "{0}{1}", (object)'k', (object)num.ToString("00000"));
            if (array.Length < 8)
                throw new FPException(FPLibError.BAD_RESPONSE);
            try
            {
                int num1 = 0;
                FP.ArticleInfo articleInfo1 = new FP.ArticleInfo();
                FP.ArticleInfo articleInfo2 = articleInfo1;
                string[] strArray1 = array;
                int index1 = num1;
                int num2 = 1;
                int num3 = index1 + num2;
                int num4 = int.Parse(strArray1[index1]);
                articleInfo2.Number = num4;
                FP.ArticleInfo articleInfo3 = articleInfo1;
                string[] strArray2 = array;
                int index2 = num3;
                int num5 = 1;
                int num6 = index2 + num5;
                string str = strArray2[index2].Trim();
                articleInfo3.Name = str;
                FP.ArticleInfo articleInfo4 = articleInfo1;
                string[] strArray3 = array;
                int index3 = num6;
                int num7 = 1;
                int num8 = index3 + num7;
                Decimal num9 = FP.s2d(strArray3[index3]);
                articleInfo4.Price = num9;
                FP.ArticleInfo articleInfo5 = articleInfo1;
                string[] strArray4 = array;
                int index4 = num8;
                int num10 = 1;
                int num11 = index4 + num10;
                int num12 = (int)strArray4[index4][0];
                articleInfo5.TaxGroup = (char)num12;
                FP.ArticleInfo articleInfo6 = articleInfo1;
                string[] strArray5 = array;
                int index5 = num11;
                int num13 = 1;
                int num14 = index5 + num13;
                Decimal num15 = FP.s2d(strArray5[index5]);
                articleInfo6.Turnover = num15;
                FP.ArticleInfo articleInfo7 = articleInfo1;
                string[] strArray6 = array;
                int index6 = num14;
                int num16 = 1;
                int num17 = index6 + num16;
                Decimal num18 = FP.s2d(strArray6[index6]);
                articleInfo7.Sells = num18;
                FP.ArticleInfo articleInfo8 = articleInfo1;
                string[] strArray7 = array;
                int index7 = num17;
                int num19 = 1;
                int num20 = index7 + num19;
                int num21 = int.Parse(strArray7[index7]);
                articleInfo8.Counter = num21;
                FP.ArticleInfo articleInfo9 = articleInfo1;
                string[] strArray8 = array;
                int index8 = num20;
                int num22 = 1;
                int num23 = index8 + num22;
                DateTime dateTime = FP.s2dt(strArray8[index8]);
                articleInfo9.LastZeroing = dateTime;
                if (array.Length > 8 && this.last_response_raw.Length > 89)
                    articleInfo1.Subgroup = this.b2i(this.last_response_raw[89]);
                return articleInfo1;
            }
            catch (Exception ex)
            {
            }
            throw new FPException(FPLibError.BAD_RESPONSE);
        }

        public FP.ArticleInfo GetSubgroupInfo(int num)
        {
            string[] array = this.SendCommandGetArray(new int[2] { -1, 20 }, "{0}{1}", (object)'g', (object)num.ToString("00"));
            if (array.Length >= 5)
            {
                try
                {
                    return new FP.ArticleInfo()
                    {
                        Name = array[1].TrimEnd(' '),
                        TaxGroup = array[2][0],
                        Turnover = FP.s2d(array[3]),
                        Sells = FP.s2d(array[4])
                    };
                }
                catch
                {
                }
            }
            throw new FPException(FPLibError.BAD_RESPONSE);
        }

        public Decimal[] GetAmountsByTaxGroup()
        {
            string[] array = this.SendCommandGetArray("{0}", (object)'m');
            if (array.Length != FP.TaxGroupCount)
                throw new FPException(FPLibError.BAD_RESPONSE);
            return FP.sa2da(array, 0, FP.TaxGroupCount);
        }

        public Decimal[] GetAmounts(char type)
        {
            string[] array = this.SendCommandGetArray("{0}{1}", new object[2]
      {
        (object) 'n',
        (object) type
      });
            if (array.Length != 6 || (int)array[0][0] != (int)type)
                throw new FPException(FPLibError.BAD_RESPONSE);
            return FP.sa2da(array, 1, 5);
        }

        public Decimal[] GetAmountsAndNumOps(char type, out int numOps)
        {
            string[] array = this.SendCommandGetArray("{0}{1}", new object[2]
      {
        (object) 'n',
        (object) type
      });
            if (array.Length != 7 || (int)array[0][0] != (int)type)
                throw new FPException(FPLibError.BAD_RESPONSE);
            numOps = int.Parse(array[6]);
            return FP.sa2da(array, 1, 5);
        }

        public FP.RegistersInfo GetRegistersInfo()
        {
            string[] array = this.SendCommandGetArray("{0}1", (object)'n');
            if (array.Length > 1 && array[0] == "1")
                return new FP.RegistersInfo(array, 1);
            throw new FPException(FPLibError.BAD_RESPONSE);
        }

        public FP.DailyReportInfo GetDailyReportInfo()
        {
            return new FP.DailyReportInfo(this.SendCommandGetArray("{0}5", (object)'n'));
        }

        public FP.OperatorReportInfo GetOperatorReportInfo(int opNum)
        {
            FP.CheckOperatorNumber(opNum);
            string[] array = this.SendCommandGetArray("{0}1;{1}", new object[2]
      {
        (object) 'o',
        (object) opNum
      });
            if (array.Length != 9 || array[0] != "1")
                throw new FPException(FPLibError.BAD_RESPONSE);
            return new FP.OperatorReportInfo(array);
        }

        public Decimal[] GetOperatorAmountsAndNumOps(int opNum, char type, out int numOps)
        {
            FP.CheckOperatorNumber(opNum);
            string[] array = this.SendCommandGetArray("{0}{1};{2}", (object)'o', (object)type, (object)opNum);
            if (array.Length != 8 || (int)array[0][0] != (int)type)
                throw new FPException(FPLibError.BAD_RESPONSE);
            numOps = int.Parse(array[7]);
            return FP.sa2da(array, 2, 5);
        }

        public Decimal[] GetOperatorAmounts(int opNum, char type)
        {
            FP.CheckOperatorNumber(opNum);
            string[] array = this.SendCommandGetArray("{0}{1};{2}", (object)'o', (object)type, (object)opNum);
            if (array.Length != 7 || (int)array[0][0] != (int)type)
                throw new FPException(FPLibError.BAD_RESPONSE);
            return FP.sa2da(array, 2, 5);
        }

        public void GetOperatorLastReport(int opNum, out int num, out DateTime time)
        {
            string[] array = this.SendCommandGetArray("{0}5;{1}", new object[2]
      {
        (object) 'o',
        (object) opNum
      });
            if (array.Length >= 4)
            {
                if ((int)array[0][0] == 53)
                {
                    try
                    {
                        num = int.Parse(array[2]);
                        time = FP.s2dt(array[3]);
                        return;
                    }
                    catch
                    {
                    }
                    throw new FPException(FPLibError.BAD_RESPONSE);
                }
            }
            throw new FPException(FPLibError.BAD_RESPONSE);
        }

        public int GetLastReceiptNumber()
        {
            string s = this.SendCommandGetString("{0}", (object)'q');
            try
            {
                return int.Parse(s);
            }
            catch
            {
            }
            throw new FPException(FPLibError.BAD_RESPONSE);
        }

        public FP.FiscalReceiptInfo GetFiscalReceiptInfo()
        {
            return new FP.FiscalReceiptInfo(this.SendCommandGetArray("r"));
        }

        public FP.LastDailyReportInfo GetLastDailyReportInfo()
        {
            return new FP.LastDailyReportInfo(this.SendCommandGetArray("{0}", (object)'s'));
        }

        public int GetFreeFiscMemBlocks()
        {
            string s = this.SendCommandGetString("{0}", (object)'t');
            try
            {
                return int.Parse(s);
            }
            catch
            {
            }
            throw new FPException(FPLibError.BAD_RESPONSE);
        }

        public byte[] ReadFiscalMemory()
        {
            List<byte> byteList = new List<byte>();
            this.SendCommand(0 != 0, new byte[1] { (byte)117 });
            byte data;
            do
            {
                try
                {
                    data = (byte)this.port.ReadByte();
                    this.log_input(data);
                }
                catch (TimeoutException ex)
                {
                    throw new FPException(FPLibError.TIMEOUT, (Exception)ex);
                }
                catch (Exception ex)
                {
                    throw new FPException(FPLibError.UNDEFINED, ex);
                }
                byteList.Add(data);
            }
            while ((int)data != 64);
            return byteList.ToArray();
        }

        public void ReportSpecialFiscal()
        {
            this.SendCommand(true, "w", new object[0]);
            this.WaitSlowCommand();
        }

        public void ReportFiscalByBlock(bool detailed, bool payments, int startBlock, int endBlock)
        {
            if (startBlock < 0 || startBlock > 9999 || (endBlock < startBlock || endBlock > 9999))
                throw new FPException(FPLibError.BAD_INPUT_DATA);
            this.SendCommand(1 != 0, "{2}{0};{1}{3}", (object)startBlock.ToString("D4"), (object)endBlock.ToString("D4"), detailed ? (object)"x" : (object)"y", payments ? (object)";P" : (object)"");
            this.WaitSlowCommand();
        }

        public void ReportFiscalByBlock(bool detailed, int startBlock, int endBlock)
        {
            this.ReportFiscalByBlock(detailed, startBlock, endBlock);
        }

        public void ReportFiscalByDate(bool detailed, bool payments, DateTime start, DateTime end)
        {
            if (int.Parse(start.ToString("yyyyMMdd")) > int.Parse(end.ToString("yyyyMMdd")))
                throw new FPException(FPLibError.BAD_INPUT_DATA);
            this.SendCommand(1 != 0, "{2}{0};{1}{3}", (object)start.ToString("ddMMyy"), (object)end.ToString("ddMMyy"), detailed ? (object)"z" : (object)"{", payments ? (object)";P" : (object)"");
            this.WaitSlowCommand();
        }

        public void ReportFiscalByDate(bool detailed, DateTime start, DateTime end)
        {
            this.ReportFiscalByDate(detailed, false, start, end);
        }

        public void ReportOperator(bool zero, int opNum)
        {
            if (opNum != 0)
                FP.CheckOperatorNumber(opNum);
            this.SendCommand(1 != 0, "}}{0};{1}", (object)FP.Flag(zero, 'Z', 'X'), (object)opNum);
            this.WaitSlowCommand();
        }

        public void ReportArticles(bool zero)
        {
            this.SendCommand(1 != 0, "~{0}", (object)(char)(zero ? 90 : 88));
            this.WaitSlowCommand();
        }

        public void ReportSubgroup(bool zero)
        {
            this.SendCommand(true, zero ? "vZ" : "vX", new object[0]);
            this.WaitSlowCommand();
        }

        public void ReportDaily(bool zero, bool extended)
        {
            this.SendCommand(1 != 0, "{0}{1}", (object)(char)(extended ? (int)sbyte.MaxValue : 124), (object)(char)(zero ? 90 : 88));
            this.WaitSlowCommand();
        }

        public void ReportEJ()
        {
            this.SendCommand(1 != 0, "{0}E", (object)'|');
            this.WaitSlowCommand();
        }

        public FP.PrinterModuleStatus GetPrinterModuleStatus()
        {
            byte[] numArray = this.SendCommand(true, "f", new object[0]);
            if (numArray.Length < 5)
                throw new FPException(FPLibError.BAD_RESPONSE);
            byte[] raw = new byte[numArray.Length - 1];
            Array.Copy((Array)numArray, 1, (Array)raw, 0, raw.Length);
            if (raw.Length >= 6 && (int)raw[1] < 128)
                raw[1] = raw[2] = raw[3] = raw[4] = (byte)0;
            return new FP.PrinterModuleStatus(raw);
        }

        public void SetPrintBarcodeInReceipt(bool enable)
        {
            this.SendCommand(1 != 0, "{0}{1}", (object)'Q', (object)FP.Flag(enable, 'E', 'D'));
        }

        public void SetBarcodeFormat(string format)
        {
            if (format.Length != 12)
                throw new FPException(FPLibError.BAD_RESPONSE);
            this.SendCommand(1 != 0, "{0}F;{1}", (object)'Q', (object)format);
        }

        public void PrintBarcode(char codeType, int codeLen, string data, bool centered)
        {
            if (data.Length > (int)byte.MaxValue)
                throw new FPException(FPLibError.BAD_RESPONSE);
            if (centered)
                this.SendCommand(1 != 0, "{0}P;{1};{2};{3};{4}", (object)'Q', (object)codeType, (object)codeLen, (object)data, (object)'1');
            else
                this.SendCommand(1 != 0, "{0}P;{1};{2};{3}", (object)'Q', (object)codeType, (object)codeLen, (object)data);
        }

        public string GSSendCommandGetString(string cmd)
        {
            byte[] bytes = this.GSSendCommand(cmd);
            return this.TextEncoding.GetString(bytes, 0, bytes.Length);
        }

        public int[] GetDatabaseRange()
        {
            int gsEndByte = this.GSEndByte;
            this.GSEndByte = 10;
            try
            {
                string[] strArray = this.GSSendCommandGetString("I").Substring(1).Split(';');
                int[] numArray = new int[strArray.Length];
                for (int index = 0; index < strArray.Length; ++index)
                {
                    try
                    {
                        numArray[index] = int.Parse(strArray[index], (IFormatProvider)CultureInfo.InvariantCulture);
                    }
                    catch
                    {
                        numArray[index] = 0;
                    }
                }
                return numArray;
            }
            catch
            {
            }
            finally
            {
                this.GSEndByte = gsEndByte;
            }
            return (int[])null;
        }

        public byte[] GSSendCommand(string cmd)
        {
            return this.GSSendCommand(this.TextEncoding.GetBytes(cmd), -1);
        }

        public byte[] GSSendCommand(byte[] data, int res_len)
        {
            int num = (int)this.Ping((byte)5, FPLibError.PRINTER_BUSY, 10, false);
            try
            {
                this.port.DiscardInBuffer();
                byte[] numArray = new byte[data.Length + 1];
                numArray[0] = (byte)29;
                Array.Copy((Array)data, 0, (Array)numArray, 1, data.Length);
                this.port.Write(numArray, 0, numArray.Length);
                this.log_output(numArray);
            }
            catch (TimeoutException ex)
            {
                throw new FPException(FPLibError.TIMEOUT, (Exception)ex);
            }
            catch (Exception ex)
            {
                throw new FPException(FPLibError.UNDEFINED, ex);
            }
            List<byte> byteList = new List<byte>();
            int readTimeout = this.port.ReadTimeout;
            while (res_len != 0)
            {
                try
                {
                    byte data1 = (byte)this.port.ReadByte();
                    this.log_input(data1);
                    if (this.GSEndByte != -1)
                    {
                        if ((int)data1 == (int)(byte)this.GSEndByte)
                            break;
                    }
                    byteList.Add(data1);
                    if (res_len > 0)
                    {
                        if (byteList.Count == res_len)
                            break;
                    }
                    if (byteList.Count == 1)
                        this.port.ReadTimeout = this.gs_timeout;
                }
                catch (TimeoutException ex)
                {
                    break;
                }
                catch (Exception ex)
                {
                    throw new FPException(FPLibError.UNDEFINED, ex);
                }
                finally
                {
                    this.port.ReadTimeout = readTimeout;
                }
            }
            return byteList.ToArray();
        }

        public string GSGetVersion()
        {
            byte[] bytes = this.GSSendCommand("?");
            return this.TextEncoding.GetString(bytes, 3, bytes.Length - 3);
        }

        public void GSCommunicationControl(bool enable, int devNo)
        {
            if (this.GSSendCommandGetString(string.Format("={0}F{1}", (object)FP.OzFlag(!enable), (object)devNo.ToString().PadLeft(4, '0'))) != "ACK")
                throw new FPException(FPLibError.BAD_RESPONSE);
        }

        public void GSSetSpeed(int speedStep)
        {
            if (speedStep > 4)
                throw new FPException(FPLibError.BAD_RESPONSE);
            this.GSSendCommand("S" + (object)speedStep);
        }

        public byte[] SendCommand(bool get_reply, string format, params object[] arg0)
        {
            byte[] bytes = this.TextEncoding.GetBytes(string.Format((IFormatProvider)CultureInfo.InvariantCulture, format, arg0));
            return this.SendCommand(get_reply, bytes);
        }

        public virtual byte[] SendCommand(bool get_reply, byte[] data_cmd)
        {
            int len = data_cmd.Length + 2;
            if (len > 250 && (int)data_cmd[0] != 77)
                throw new FPException(FPLibError.BAD_INPUT_DATA);
            byte[] numArray = new byte[data_cmd.Length + 6];
            this.cmd_id = (byte)((int)++FP.next_cmd_id % (int)sbyte.MaxValue + 32);
            numArray[0] = (byte)2;
            numArray[1] = (byte)(len + 32);
            numArray[2] = this.cmd_id;
            Array.Copy((Array)data_cmd, 0, (Array)numArray, 3, data_cmd.Length);
            this.MakeCRC(numArray, 1, len);
            numArray[numArray.Length - 1] = (byte)10;
            this.last_response_raw = (byte[])null;
            int num = (int)this.Ping((byte)5, FPLibError.PRINTER_BUSY, 10, true);
            try
            {
                this.port.DiscardInBuffer();
                this.port.Write(numArray, 0, numArray.Length);
                this.log_output(numArray);
            }
            catch (Exception ex)
            {
                if (ex.GetType() == typeof(TimeoutException))
                    throw new FPException(FPLibError.TIMEOUT, ex);
                throw new FPException(FPLibError.UNDEFINED, ex);
            }
            FPException.last_error = FPLibError.SUCCESS;
            if (!get_reply)
                return (byte[])null;
            this.last_response_raw = this.GetResponse(numArray);
            return this.last_response_raw;
        }

        public string[] SendCommandGetArray(int[] fixed_size, string format, params object[] arg0)
        {
            string str1 = this.SendCommandGetString(format, arg0);
            ArrayList arrayList = new ArrayList();
            for (int index = 0; index < fixed_size.Length; ++index)
            {
                if (str1.Length >= fixed_size[index])
                {
                    int num = fixed_size[index];
                    if (num == -1)
                    {
                        num = str1.IndexOf(';');
                        if (num == -1)
                            num = str1.Length;
                    }
                    arrayList.Add((object)str1.Substring(0, num));
                    str1 = str1.Substring(num);
                    if (str1.Length > 0 && (int)str1[0] == 59)
                        str1 = str1.Substring(1);
                }
            }
            if (str1.Length > 0)
            {
                string str2 = str1;
                char[] chArray = new char[1] { ';' };
                foreach (string str3 in str2.Split(chArray))
                    arrayList.Add((object)str3.Trim());
            }
            return (string[])arrayList.ToArray(typeof(string));
        }

        public string[] SendCommandGetArray(string format, params object[] arg0)
        {
            string str1 = this.SendCommandGetString(format, arg0);
            if (str1 == null)
                return new string[0];
            List<string> stringList = new List<string>();
            string str2 = str1;
            char[] chArray1 = new char[1] { ';' };
            foreach (string str3 in str2.Split(chArray1))
            {
                char[] chArray2 = new char[2] { ' ', ';' };
                string str4 = str3.Trim(chArray2);
                stringList.Add(str4);
            }
            return stringList.ToArray();
        }

        public string SendCommandGetString(string format, params object[] arg0)
        {
            byte[] bytes = this.SendCommand(true, format, arg0);
            if (bytes == null)
                return "";
            string str = this.TextEncoding.GetString(bytes, 0, bytes.Length);
            if (str.Length <= 1)
                throw new FPException(FPLibError.BAD_RESPONSE);
            return str.Substring(1).Trim(';');
        }

        public byte[] GetResponse(byte[] cmd)
        {
            ArrayList arrayList = new ArrayList();
            int num1 = 3;
            do
            {
                int num2;
                try
                {
                    num2 = this.port.ReadByte();
                    if (num2 != -1)
                        this.log_input((byte)num2);
                }
                catch (Exception ex)
                {
                    if (ex.GetType() == typeof(TimeoutException))
                        throw new FPException(FPLibError.TIMEOUT, ex);
                    throw new FPException(FPLibError.UNDEFINED, ex);
                }
                if (arrayList.Count == 0)
                {
                    if (num2 == 3)
                        throw new FPException(FPLibError.UNKNOWN_DEVICE);
                    if (num2 == 21)
                        throw new FPException(FPLibError.NACK);
                    if (num2 == 14)
                    {
                        if (num1 <= 0 || cmd == null)
                            throw new FPException(FPLibError.RETRIED);
                        --num1;
                        arrayList.Clear();
                        Thread.Sleep(50);
                        this.port.Write(cmd, 0, cmd.Length);
                        this.log_output(cmd);
                        continue;
                    }
                }
                else
                {
                    if (arrayList.Count == 1 && (int)(byte)arrayList[0] == 2)
                    {
                        int num3 = num2 - 32 & (int)sbyte.MaxValue;
                        if (num3 > 1)
                        {
                            byte[] numArray = new byte[num3 - 1];
                            int count = this.port.Read(numArray, 0, numArray.Length);
                            this.log_input(numArray, count);
                            arrayList.Add((object)(byte)num2);
                            for (int index = 0; index < count; ++index)
                                arrayList.Add((object)numArray[index]);
                            continue;
                        }
                    }
                    if (num2 == 10)
                    {
                        if (arrayList.Count < 5)
                            throw new FPException(FPLibError.BAD_RESPONSE);
                        byte[] array = (byte[])arrayList.ToArray(typeof(byte));
                        byte num3 = array[array.Length - 2];
                        byte num4 = array[array.Length - 1];
                        this.MakeCRC(array, 1, array.Length - 3);
                        if ((int)array[array.Length - 2] != (int)num3 || (int)array[array.Length - 1] != (int)num4)
                            throw new FPException(FPLibError.CRC);
                        if ((int)array[0] == 6)
                        {
                            byte num5 = array[array.Length - 4];
                            byte num6 = array[array.Length - 3];
                            byte num7 = (byte)(((int)num5 <= 57 ? (int)num5 : ((int)num5 <= 63 ? (int)num5 - 48 : (int)num5 - 55)) << 4 | ((int)num6 <= 57 ? (int)num6 : ((int)num6 <= 63 ? (int)num6 - 48 : (int)num6 - 55)) & 15);
                            if ((int)num7 != 0)
                                throw new FPException((int)num7);
                            return (byte[])null;
                        }
                        if ((int)array[2] != (int)this.cmd_id)
                            throw new FPException(FPLibError.NBL_NOT_SAME);
                        if ((int)array[1] - 32 != array.Length - 3)
                            throw new FPException(FPLibError.BAD_RESPONSE);
                        byte[] numArray = new byte[arrayList.Count - 5];
                        arrayList.CopyTo(3, (Array)numArray, 0, numArray.Length);
                        return numArray;
                    }
                }
                arrayList.Add((object)(byte)num2);
                if (arrayList.Count > 6 && (int)(byte)arrayList[0] == 6)
                    throw new FPException(FPLibError.BAD_RECEIPT);
            }
            while (arrayList.Count <= (int)byte.MaxValue);
            throw new FPException(FPLibError.BAD_RESPONSE);
        }

        private FPLibError Ping(byte ping_code, FPLibError res_default, int retries, bool throw_exception)
        {
            FPException.last_error = FPLibError.SUCCESS;
            if (this.port == null)
            {
                if (throw_exception)
                    throw new FPException(FPLibError.PORT_NOT_OPEN);
                return FPLibError.PORT_NOT_OPEN;
            }
            byte num1 = 3;
            int readTimeout = this.port.ReadTimeout;
            bool? newPing1 = this.new_ping;
            byte[] numArray;
            if ((!newPing1.GetValueOrDefault() ? 0 : (newPing1.HasValue ? 1 : 0)) != 0)
            {
                numArray = new byte[1] { (byte)9 };
                num1 = (byte)9;
            }
            else
            {
                numArray = new byte[2]
        {
          (byte) 3,
          ping_code
        };
                if (!this.new_ping.HasValue)
                    numArray[0] = (byte)9;
            }
            this.port.DiscardInBuffer();
            for (int index = 0; index < retries; ++index)
            {
                int num2 = 0;
                this.port.Write(numArray, 0, numArray.Length);
                this.log_output(numArray);
                this.port.ReadTimeout = this.ping_timeout;
                try
                {
                    num2 = this.port.ReadByte();
                    if (num2 != -1)
                        this.log_input((byte)num2);
                    if (!this.new_ping.HasValue)
                    {
                        if ((num2 & 192) == 64)
                        {
                            this.new_ping = new bool?(true);
                            num2 = this.port.ReadByte();
                            if (num2 != -1)
                                this.log_input((byte)num2);
                        }
                        else
                            this.new_ping = new bool?(false);
                    }
                }
                catch
                {
                }
                this.port.ReadTimeout = readTimeout;
                if (num2 == (int)ping_code)
                    return FPLibError.SUCCESS;
                bool? newPing2 = this.new_ping;
                if ((!newPing2.GetValueOrDefault() ? 0 : (newPing2.HasValue ? 1 : 0)) != 0 && (num2 & 192) == 64)
                {
                    if ((num2 & 1) != 0 && (int)ping_code == 5)
                    {
                        res_default = FPLibError.PRINTER_BUSY;
                    }
                    else
                    {
                        if ((num2 & 2) == 0)
                            return FPLibError.SUCCESS;
                        res_default = FPLibError.PAPER_EMPTY;
                    }
                }
                if (num2 == 7)
                    res_default = FPLibError.PAPER_EMPTY;
                if (num2 == (int)num1)
                {
                    if (throw_exception)
                        throw new FPException(FPLibError.UNKNOWN_DEVICE);
                    return FPLibError.UNKNOWN_DEVICE;
                }
            }
            if (res_default != FPLibError.SUCCESS)
                FPException.last_error = res_default;
            if (throw_exception)
                throw new FPException(res_default);
            return res_default;
        }

        private void MakeCRC(byte[] data, int start, int len)
        {
            byte num = 0;
            for (int index = 0; index < len; ++index)
                num ^= data[start + index];
            data[start + len] = (byte)((int)num >> 4 | 48);
            data[start + len + 1] = (byte)((int)num & 15 | 48);
        }

        public bool WaitIfBusy(int timeout, bool throw_exception)
        {
            DateTime dateTime = DateTime.Now.AddMilliseconds((double)timeout);
            while (this.IsPrinterBusy)
            {
                Thread.Sleep(100);
                if (timeout >= 0 && DateTime.Now.CompareTo(dateTime) >= 0)
                {
                    if (throw_exception)
                        throw new FPException(FPLibError.BUSY_TIMEOUT);
                    return true;
                }
            }
            return false;
        }

        public void OpenPort(string port_name, int baud_rate, int num_ping_retries)
        {
            this.ClosePort();
            SerialPort p = new SerialPort(port_name, baud_rate);
            p.Open();
            this.OpenPort(p, num_ping_retries);
        }

        public void OpenPort(SerialPort p, int num_ping_retries)
        {
            if (FP.PortLog != null)
                FP.PortLog((byte[])null, this.TextEncoding.CodePage, FP.LogEventType.OpenPort);
            this.port = p;
            this.port.ReadTimeout = this.rw_timeout;
            this.port.WriteTimeout = this.rw_timeout;
            int num = (int)this.Ping((byte)4, FPLibError.NO_PRINTER, num_ping_retries, true);
        }

        public void OpenPort(string port_name, int baud_rate)
        {
            this.OpenPort(port_name, baud_rate, 10);
        }

        public void ClosePort()
        {
            if (this.port != null)
            {
                if (this.port.IsOpen)
                {
                    this.port.Close();
                    if (FP.PortLog != null)
                        FP.PortLog((byte[])null, this.TextEncoding.CodePage, FP.LogEventType.ClosePort);
                }
                this.port = (SerialPort)null;
            }
            this.new_ping = new bool?();
        }

        public void Dispose()
        {
            this.ClosePort();
            if (this.port == null)
                return;
            this.port.Dispose();
        }

        private static string fix_len(string text, int len, char padding)
        {
            return text.PadRight(len, padding).Substring(0, len);
        }

        private static string fix_len(string text, int len)
        {
            return FP.fix_len(text, len, ' ');
        }

        private static DateTime s2dt(string str)
        {
            return DateTime.ParseExact(str, new string[2]
      {
        "dd-MM-yyyy",
        "dd-MM-yyyy HH:mm"
      }, (IFormatProvider)CultureInfo.InvariantCulture, DateTimeStyles.AllowTrailingWhite);
        }

        private static string d2s(Decimal value, int decimals)
        {
            return string.Format((IFormatProvider)CultureInfo.InvariantCulture, "{0}", new object[1]
      {
        (object) Decimal.Round(value, decimals)
      });
        }

        private static string d2s(Decimal value)
        {
            return FP.d2s(value, 2);
        }

        private static Decimal s2d(string str)
        {
            return Decimal.Parse(str.Trim(' ', ';'), (IFormatProvider)CultureInfo.InvariantCulture);
        }

        private int b2i(byte val)
        {
            if ((int)val < 48)
                return (int)val;
            if ((int)val < 128)
                return (int)val - 48;
            return (int)val - 128;
        }

        private byte i2b(int val)
        {
            if (val < 10)
                return (byte)(val + 48);
            return (byte)(val + 128);
        }

        private static Decimal[] sa2da(string[] ss, int startIndex, int count)
        {
            if (ss.Length < startIndex + count)
                throw new FPException(FPLibError.BAD_RESPONSE);
            Decimal[] numArray = new Decimal[count];
            for (int index = 0; index < count; ++index)
                numArray[index] = FP.s2d(ss[startIndex + index]);
            return numArray;
        }

        private static void CheckOperatorNumber(int opNum)
        {
            if (opNum < 1 || opNum > 99)
                throw new FPException(FPLibError.BAD_INPUT_DATA);
        }

        private static char Flag(bool val, char forTrue, char forFalse)
        {
            if (!val)
                return forFalse;
            return forTrue;
        }

        private static char OzFlag(bool val)
        {
            return FP.Flag(val, '1', '0');
        }

        private void log_input(byte data)
        {
            if (FP.PortLog == null)
                return;
            byte[] data1 = new byte[1] { data };
            FP.PortLog(data1, this.TextEncoding.CodePage, FP.LogEventType.Read);
        }

        private void log_input(byte[] data, int count)
        {
            if (FP.PortLog == null || count <= 0)
                return;
            if (data.Length > count)
            {
                byte[] data1 = new byte[count];
                Array.Copy((Array)data, 0, (Array)data1, 0, count);
                FP.PortLog(data1, this.TextEncoding.CodePage, FP.LogEventType.Read);
            }
            else
                FP.PortLog(data, this.TextEncoding.CodePage, FP.LogEventType.Read);
        }

        private void log_output(byte[] data)
        {
            if (FP.PortLog == null)
                return;
            FP.PortLog(data, this.TextEncoding.CodePage, FP.LogEventType.Write);
        }

        private void WaitSlowCommand()
        {
            if (!this.WaitAfterSlowCommands)
                return;
            this.WaitIfBusy(20000, false);
        }

        public delegate void LogEventHandler(byte[] data, int code_page, FP.LogEventType type);

        public class ArticleInfo
        {
            public int Number { get; set; }

            public string Name { get; set; }

            public Decimal Price { get; set; }

            public char TaxGroup { get; set; }

            public int Subgroup { get; set; }

            public Decimal Turnover { get; set; }

            public Decimal Sells { get; set; }

            public int Counter { get; set; }

            public DateTime LastZeroing { get; set; }

            public ArticleInfo()
            {
            }

            public ArticleInfo(int num, string name, Decimal price, char taxGroup)
            {
                this.Number = num;
                this.Name = name;
                this.Price = price;
                this.TaxGroup = taxGroup;
            }
        }

        public class PayTypes
        {
            public string[] Names { get; set; }

            public string CurrencyName { get; set; }

            public Decimal CurrencyExchRate { get; set; }

            public PayTypes(string[] vals)
            {
                if (vals.Length < 6)
                    throw new FPException(FPLibError.BAD_INPUT_DATA);
                this.Names = new string[4];
                for (int index = 0; index < 4; ++index)
                    this.Names[index] = vals[index];
                this.CurrencyName = vals[4];
                this.CurrencyExchRate = Decimal.Parse(vals[5], (IFormatProvider)CultureInfo.InvariantCulture);
            }
        }

        public class Parameters
        {
            private string[] raw;

            public int POSNumber
            {
                get
                {
                    return int.Parse(this.raw[0]);
                }
            }

            public bool PrintLogo
            {
                get
                {
                    return (int)this.raw[1][0] == 49;
                }
                set
                {
                    this.raw[1] = value ? "1" : "0";
                }
            }

            public bool OpenCashDrawer
            {
                get
                {
                    return (int)this.raw[2][0] == 49;
                }
            }

            public bool AutoCut
            {
                get
                {
                    return (int)this.raw[3][0] == 49;
                }
            }

            public bool TransparentDisplay
            {
                get
                {
                    return (int)this.raw[4][0] == 49;
                }
            }

            public bool ShortEJ
            {
                get
                {
                    return (int)this.raw[5][0] == 49;
                }
            }

            public bool TotalInForeignCurrency
            {
                get
                {
                    return (int)this.raw[6][0] == 49;
                }
            }

            public bool SmallFontEJ
            {
                get
                {
                    if (this.raw.Length > 7)
                        return (int)this.raw[7][0] == 49;
                    return false;
                }
            }

            public bool FreeTextInEJ
            {
                get
                {
                    if (this.raw.Length > 8)
                        return (int)this.raw[8][0] == 49;
                    return false;
                }
            }

            public bool SingleOperator
            {
                get
                {
                    if (this.raw.Length > 9)
                        return (int)this.raw[9][0] == 49;
                    return false;
                }
            }

            public Parameters(string[] values)
            {
                if (values.Length < FP.DeviceParamsCount)
                    throw new FPException(FPLibError.BAD_INPUT_DATA);
                this.raw = values;
            }
        }

        public class OperatorInfo
        {
            public int Number { get; set; }

            public string Name { get; set; }

            public string Password { get; set; }

            public OperatorInfo(string[] raw)
            {
                if (raw.Length < 3)
                    throw new FPException(FPLibError.BAD_RESPONSE);
                this.Number = int.Parse(raw[0]);
                this.Name = raw[1].Trim();
                this.Password = raw[2];
            }

            public OperatorInfo(int num, string name, string pwd)
            {
                this.Number = num;
                this.Name = name;
                this.Password = pwd;
            }
        }

        public class LogosInfo
        {
            private const int MaxLogos = 10;
            private string[] raw;

            public int ActiveLogo
            {
                get
                {
                    return int.Parse(this.raw[0]);
                }
            }

            public LogosInfo(string[] vals)
            {
                if (vals.Length != 2 || vals[0].Length != 1 || vals[1].Length != 10)
                    throw new FPException(FPLibError.BAD_RESPONSE);
                this.raw = vals;
            }

            public bool IsLoaded(int index)
            {
                if (index >= 10)
                    throw new FPException(FPLibError.BAD_INPUT_DATA);
                return (int)this.raw[1][index] == 49;
            }
        }

        public class RegistersInfo
        {
            public int NumCustomers { get; set; }

            public int NumDiscounts { get; set; }

            public Decimal DiscountsTotal { get; set; }

            public int NumAdditions { get; set; }

            public Decimal AdditionsTotal { get; set; }

            public int NumVoids { get; set; }

            public Decimal VoidsTotal { get; set; }

            public RegistersInfo(string[] raw, int ix)
            {
                if (raw.Length < ix + 7)
                    throw new FPException(FPLibError.BAD_RESPONSE);
                this.NumCustomers = int.Parse(raw[ix++]);
                this.NumDiscounts = int.Parse(raw[ix++]);
                this.DiscountsTotal = FP.s2d(raw[ix++]);
                this.NumAdditions = int.Parse(raw[ix++]);
                this.AdditionsTotal = FP.s2d(raw[ix++]);
                this.NumVoids = int.Parse(raw[ix++]);
                this.VoidsTotal = FP.s2d(raw[ix++]);
            }
        }

        public class DailyReportInfo
        {
            public int LastReport { get; set; }

            public int LastFiscMemBlock { get; set; }

            public int NumEJ { get; set; }

            public DateTime LastSaveTime { get; set; }

            public DailyReportInfo(string[] raw)
            {
                if (raw.Length < 5 || raw[0] != "5")
                    throw new FPException(FPLibError.BAD_RESPONSE);
                int num1 = 1;
                string[] strArray1 = raw;
                int index1 = num1;
                int num2 = 1;
                int num3 = index1 + num2;
                this.LastReport = int.Parse(strArray1[index1]);
                string[] strArray2 = raw;
                int index2 = num3;
                int num4 = 1;
                int num5 = index2 + num4;
                this.LastFiscMemBlock = int.Parse(strArray2[index2]);
                string[] strArray3 = raw;
                int index3 = num5;
                int num6 = 1;
                int num7 = index3 + num6;
                this.NumEJ = int.Parse(strArray3[index3]);
                string[] strArray4 = raw;
                int index4 = num7;
                int num8 = 1;
                int num9 = index4 + num8;
                this.LastSaveTime = FP.s2dt(strArray4[index4]);
            }
        }

        public class OperatorReportInfo
        {
            public int OpNum { get; set; }

            public FP.RegistersInfo RInfo { get; set; }

            public OperatorReportInfo(string[] raw)
            {
                this.OpNum = int.Parse(raw[1]);
                this.RInfo = new FP.RegistersInfo(raw, 2);
            }
        }

        public class LastDailyReportInfo
        {
            public DateTime LastDate { get; set; }

            public int LastDailyReport { get; set; }

            public int LastRamReset { get; set; }

            public LastDailyReportInfo(string[] raw)
            {
                if (raw.Length < 3)
                    throw new FPException(FPLibError.BAD_RESPONSE);
                int num1 = 0;
                string[] strArray1 = raw;
                int index1 = num1;
                int num2 = 1;
                int num3 = index1 + num2;
                this.LastDate = FP.s2dt(strArray1[index1]);
                string[] strArray2 = raw;
                int index2 = num3;
                int num4 = 1;
                int num5 = index2 + num4;
                this.LastDailyReport = int.Parse(strArray2[index2]);
                string[] strArray3 = raw;
                int index3 = num5;
                int num6 = 1;
                int num7 = index3 + num6;
                this.LastRamReset = int.Parse(strArray3[index3]);
            }
        }

        public class PrinterModuleStatus
        {
            private byte[] raw;

            public bool DrawerSignalLevel
            {
                get
                {
                    return this.IsHigh(1, 2);
                }
            }

            public bool Offline
            {
                get
                {
                    return this.IsHigh(1, 3);
                }
            }

            public bool CoverOpen
            {
                get
                {
                    return this.IsHigh(1, 5);
                }
            }

            public bool PaperFeedStatus
            {
                get
                {
                    return this.IsHigh(1, 6);
                }
            }

            public bool AutoCutterError
            {
                get
                {
                    return this.IsHigh(2, 3);
                }
            }

            public bool FatalError
            {
                get
                {
                    return this.IsHigh(2, 5);
                }
            }

            public bool PaperNearEnd
            {
                get
                {
                    return this.IsHigh(3, 1);
                }
            }

            public bool OutOfPaper
            {
                get
                {
                    return this.IsHigh(3, 3);
                }
            }

            public bool LineDisplay
            {
                get
                {
                    return (int)this.raw[0] == 89;
                }
            }

            public bool ServiceJumper
            {
                get
                {
                    if (this.raw.Length >= 6)
                        return (int)this.raw[5] == 74;
                    return false;
                }
            }

            public PrinterModuleStatus(byte[] raw)
            {
                if (raw.Length < 4)
                    throw new FPException(FPLibError.BAD_RESPONSE);
                this.raw = raw;
            }

            private bool IsHigh(int byteIndex, int bitIndex)
            {
                return ((int)this.raw[byteIndex] & 1 << bitIndex) != 0;
            }
        }

        public enum LogEventType
        {
            OpenPort,
            Read,
            Write,
            ClosePort,
        }

        public class Status
        {
            public byte[] raw_data;

            public bool ReadOnlyFiscalMemory
            {
                get
                {
                    return this.IsHigh(0, 0);
                }
            }

            public bool PowerDownWhileFiscalReceiptOpen
            {
                get
                {
                    return this.IsHigh(0, 1);
                }
            }

            public bool PrinterOverheat
            {
                get
                {
                    return this.IsHigh(0, 2);
                }
            }

            public bool IncorectClock
            {
                get
                {
                    return this.IsHigh(0, 3);
                }
            }

            public bool IncorectDate
            {
                get
                {
                    return this.IsHigh(0, 4);
                }
            }

            public bool RAMError
            {
                get
                {
                    return this.IsHigh(0, 5);
                }
            }

            public bool ClockFailure
            {
                get
                {
                    return this.IsHigh(0, 6);
                }
            }

            public bool PaperOut
            {
                get
                {
                    return this.IsHigh(1, 0);
                }
            }

            public bool ReportsAccumulationOverflow
            {
                get
                {
                    return this.IsHigh(1, 1);
                }
            }

            public bool NonZeroDailyReport
            {
                get
                {
                    return this.IsHigh(1, 3);
                }
            }

            public bool NonZeroArticleReport
            {
                get
                {
                    return this.IsHigh(1, 4);
                }
            }

            public bool NonZeroOperatorReport
            {
                get
                {
                    return this.IsHigh(1, 5);
                }
            }

            public bool NonPrintedCopy
            {
                get
                {
                    return this.IsHigh(1, 6);
                }
            }

            public bool OpenNonFiscalReceipt
            {
                get
                {
                    return this.IsHigh(2, 0);
                }
            }

            public bool OpenFiscalReceipt
            {
                get
                {
                    return this.IsHigh(2, 1);
                }
            }

            public bool StandardCashReceipt
            {
                get
                {
                    return this.IsHigh(2, 2);
                }
            }

            public bool VATIncludedInReceipt
            {
                get
                {
                    return this.IsHigh(2, 3);
                }
            }

            public bool NoFiscalMemory
            {
                get
                {
                    return this.IsHigh(3, 0);
                }
            }

            public bool FiscalMemoryFailure
            {
                get
                {
                    return this.IsHigh(3, 1);
                }
            }

            public bool FiscalMemoryOverflow
            {
                get
                {
                    return this.IsHigh(3, 2);
                }
            }

            public bool FiscalMemoryAlmostFull
            {
                get
                {
                    return this.IsHigh(3, 3);
                }
            }

            public bool FractionAmmounts
            {
                get
                {
                    return this.IsHigh(3, 4);
                }
            }

            public bool Fiscalized
            {
                get
                {
                    return this.IsHigh(3, 5);
                }
            }

            public bool ServiceJumper
            {
                get
                {
                    return this.IsHigh(4, 6);
                }
            }

            public bool FactoryNumberSet
            {
                get
                {
                    return this.IsHigh(3, 6);
                }
            }

            public bool AutoCut
            {
                get
                {
                    return this.IsHigh(4, 0);
                }
            }

            public bool TransparentDisplay
            {
                get
                {
                    return this.IsHigh(4, 1);
                }
            }

            public int CommunicationSpeed
            {
                get
                {
                    return this.IsHigh(4, 2) ? 9600 : 19200;
                }
            }

            public bool AutoOpenCashDrawer
            {
                get
                {
                    return this.IsHigh(4, 4);
                }
            }

            public bool IncludeLogoInReceipt
            {
                get
                {
                    return this.IsHigh(4, 5);
                }
            }

            public Status()
            {
                this.raw_data = new byte[5];
            }

            public Status(byte[] data)
            {
                this.Load(data);
            }

            public void Load(byte[] data)
            {
                if (data == null || data.Length < 6)
                    throw new FPException(FPLibError.BAD_RESPONSE);
                this.raw_data = new byte[data.Length - 1];
                Array.Copy((Array)data, 1, (Array)this.raw_data, 0, this.raw_data.Length);
            }

            private bool IsHigh(int byteIndex, int bitIndex)
            {
                return ((int)this.raw_data[byteIndex] & 1 << bitIndex) != 0;
            }
        }

        public class FiscalReceiptInfo
        {
            public Decimal Change { get; set; }

            public FPPaymentType ChangeType { get; set; }

            public bool IsOpen { get; set; }

            public bool ForbiddenVoid { get; set; }

            public bool PrintVAT { get; set; }

            public bool PrintDetailedReceipt { get; set; }

            public bool PaymentInitiated { get; set; }

            public bool PaymentCompleted { get; set; }

            public bool IsInvoice { get; set; }

            public bool PowerDown { get; set; }

            public int SalesCount { get; set; }

            public Decimal[] TaxGroupSubtotal { get; set; }

            public FiscalReceiptInfo(string[] raw_str)
            {
                if (raw_str.Length <= 1)
                    throw new FPException(FPLibError.BAD_RESPONSE);
                int num1 = 0;
                string[] strArray1 = raw_str;
                int index1 = num1;
                int num2 = 1;
                int num3 = index1 + num2;
                this.IsOpen = strArray1[index1] == "1";
                this.TaxGroupSubtotal = new Decimal[FP.TaxGroupCount];
                if (!this.IsOpen)
                    return;
                int num4;
                switch (raw_str.Length)
                {
                    case 12:
                    case 19:
                        num4 = 3;
                        break;
                    case 16:
                    case 17:
                        num4 = 5;
                        break;
                    default:
                        throw new FPException(FPLibError.BAD_RESPONSE);
                }
                string[] strArray2 = raw_str;
                int index2 = num3;
                int num5 = 1;
                int num6 = index2 + num5;
                this.SalesCount = int.Parse(strArray2[index2]);
                int index3;
                for (index3 = 0; index3 < num4; ++index3)
                    this.TaxGroupSubtotal[index3] = FP.s2d(raw_str[num6++]);
                string[] strArray3 = raw_str;
                int index4 = num6;
                int num7 = 1;
                int num8 = index4 + num7;
                this.ForbiddenVoid = strArray3[index4] == "1";
                string[] strArray4 = raw_str;
                int index5 = num8;
                int num9 = 1;
                int num10 = index5 + num9;
                this.PrintVAT = strArray4[index5] == "1";
                string[] strArray5 = raw_str;
                int index6 = num10;
                int num11 = 1;
                int num12 = index6 + num11;
                this.PrintDetailedReceipt = strArray5[index6] == "1";
                string[] strArray6 = raw_str;
                int index7 = num12;
                int num13 = 1;
                int num14 = index7 + num13;
                this.PaymentInitiated = strArray6[index7] == "1";
                string[] strArray7 = raw_str;
                int index8 = num14;
                int num15 = 1;
                int num16 = index8 + num15;
                this.PaymentCompleted = strArray7[index8] == "1";
                string[] strArray8 = raw_str;
                int index9 = num16;
                int num17 = 1;
                int num18 = index9 + num17;
                this.PowerDown = strArray8[index9] == "1";
                string[] strArray9 = raw_str;
                int index10 = num18;
                int num19 = 1;
                int num20 = index10 + num19;
                this.IsInvoice = strArray9[index10] == "1";
                string[] strArray10 = raw_str;
                int index11 = num20;
                int num21 = 1;
                int num22 = index11 + num21;
                this.Change = FP.s2d(strArray10[index11]);
                string[] strArray11 = raw_str;
                int index12 = num22;
                int num23 = 1;
                int num24 = index12 + num23;
                this.ChangeType = (FPPaymentType)int.Parse(strArray11[index12]);
                while (num24 < raw_str.Length && index3 < FP.TaxGroupCount)
                    this.TaxGroupSubtotal[index3++] = FP.s2d(raw_str[num24++]);
            }
        }
    }
}
