using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml;
using Microsoft.Win32;
using System.IO;
using System.Globalization;

namespace LangProg_lab_10_12
{
    public partial class MainWindow : Window
    {
        XDocument XMLOurErrors;
        XElement totalRootElement;
        string XMLpathOurShema = "..\\..\\Source\\ourErrors.xml", XMLpathShema = "";

        public MainWindow()
        {
            InitializeComponent();
        }


        private void btnOpenSource_Click(object sender, RoutedEventArgs e)
        {
            btnCompare.IsEnabled = true;
            btnSaveResult.IsEnabled = true;

            string XMLpath;
            string msgBoxContent, msgBoxCaption;
            MessageBoxButton msgBoxButton;
            MessageBoxImage msgBoxImg;

            msgBoxContent = "Извините, но файл не прошёл проверку достоверности!";
            msgBoxCaption = "Ошибка";
            msgBoxButton = MessageBoxButton.OK;
            msgBoxImg = MessageBoxImage.Error;
            XMLOurErrors = new XDocument();

            XMLpath = read_xml_from_file();

            if (!(String.IsNullOrEmpty(XMLpath)))
            {
                txtBoxSourceFile.Text = "";
                using (StreamReader sr = new StreamReader(XMLpath, true))
                {
                    XDocument XMLsource = XDocument.Load(sr);

                    txtBoxSourceFile.Text += XMLsource;

                    XmlSchemaSet schemaSet = new XmlSchemaSet();
                    schemaSet.Add(null, "..\\..\\Schema\\schema.xsd");

                    try
                    {
                        XMLsource.Validate(schemaSet, null);
                        txtBoxErrorsSource.Text = "Проверка достоверности документа успешно завершена!\n";

                        totalRootElement = new XElement("Ошибки_в_XML_файле");
                        XMLOurErrors.Add(totalRootElement);

                        check_unrecorded_revenue_and_tax_deductions(XMLsource);
                        check_countable_taxes(XMLsource);
                        check_countable_taxes_and_advance_payments(XMLsource);
                        check_income_earning_and_tax_deduction(XMLsource);
                        check_date_of_transfer_and_tax_deduction(XMLsource);
                        check_sum_withheld_tax_and_sum_actually_received_income(XMLsource);
                        check_sum_withheld_tax_section1_and_section2(XMLsource);
                        
                        if (System.IO.Directory.Exists("..\\..\\Source") == false) Directory.CreateDirectory("..\\..\\Source");

                        XMLOurErrors.Save("..\\..\\Source\\ourErrors.xml");
                    }
                    catch (XmlSchemaValidationException ex)
                    {
                        txtBoxErrorsSource.Text = "Произошло исключение: " + ex.Message + ". \nДокумент не прошел проверку достоверности.";
                        MessageBox.Show(msgBoxContent, msgBoxCaption, msgBoxButton, msgBoxImg);
                        btnCompare.IsEnabled = false;
                        btnSaveResult.IsEnabled = false;
                        txtBoxCompareResult.Text = "";
                    }
                }
            }
        }

        private void btnOpenShema_Click(object sender, RoutedEventArgs e)
        {
            btnCompare.Content = "Сравнить шаблоны";
            string msgBoxContent, msgBoxCaption;
            MessageBoxButton msgBoxButton;
            MessageBoxImage msgBoxImg;

            msgBoxContent = "Извините, но файл не прошёл проверку достоверности!";
            msgBoxCaption = "Ошибка";
            msgBoxButton = MessageBoxButton.OK;
            msgBoxImg = MessageBoxImage.Error;

            string XMLpath;

            XMLpath = read_xml_from_file();

            if (!(String.IsNullOrEmpty(XMLpath)))
            {
                XMLpathShema = XMLpath;
                txtBoxSchema.Text = "";
                using (StreamReader source = new StreamReader(XMLpath, true))
                {
                    try
                    {
                        XDocument XMLsource = XDocument.Load(source);
                        txtBoxSchema.Text += XMLsource;
                    }
                    catch
                    {
                        MessageBox.Show(msgBoxContent, msgBoxCaption, msgBoxButton, msgBoxImg);
                    }
                }
            }
        }
        
        private void btnSaveResult_Click(object sender, RoutedEventArgs e)
        {
            string SaveXmlPath = save_xml_in_file();
            if (!(String.IsNullOrEmpty(SaveXmlPath)))
            {
                StreamWriter File = new StreamWriter(SaveXmlPath);
                File.WriteLine(txtBoxCompareResult.Text);
                File.Close();
            }
        }

        private void btnClearShema_Click(object sender, RoutedEventArgs e)
        {
            txtBoxSchema.Text = "";
            btnCompare.Content = "Проверить";
            XMLpathShema = null;
        }

        private void btnCompare_Click(object sender, RoutedEventArgs e)
        {
            txtBoxCompareResult.Text = "";
            int[] Control = new int[7];
            bool Found = false;
            int Counter = 0;

            if (String.IsNullOrEmpty(XMLpathShema))
            {
                using (StreamReader sr = new StreamReader(XMLpathOurShema, true))
                {
                    XDocument XMLsource = XDocument.Load(sr);

                    txtBoxCompareResult.Text += XMLsource;
                }
            }
            else
            {
                XDocument XMLOurShema;

                using (StreamReader OurSR = new StreamReader(XMLpathOurShema, true))
                {
                    XMLOurShema = XDocument.Load(OurSR);
                }

                XDocument XMLShema;

                using (StreamReader SR = new StreamReader(XMLpathShema, true))
                {
                    XMLShema = XDocument.Load(SR);
                }

                for (int i = 0; i < 7; i++) Control[i] = 2;

                foreach (var OurXMLFile in XMLOurShema.Descendants())
                {
                    Found = false;

                    if (OurXMLFile.Name == "Ошибки_в_XML_файле" || OurXMLFile.Name == "Ошибки_вида_02" || OurXMLFile.Name == "Ошибки_вида_03" || OurXMLFile.Name == "Ошибки_вида_04" || OurXMLFile.Name == "Ошибки_вида_A" || OurXMLFile.Name == "Ошибки_вида_B" || OurXMLFile.Name == "Ошибки_вида_C" || OurXMLFile.Name == "Ошибки_вида_D")
                    {
                        switch (Convert.ToString(OurXMLFile.Name))
                        {
                            case "Ошибки_вида_02": Counter = 0; break;
                            case "Ошибки_вида_03": Counter = 1; break;
                            case "Ошибки_вида_04": Counter = 2; break;
                            case "Ошибки_вида_A": Counter = 3; break;
                            case "Ошибки_вида_B": Counter = 4; break;
                            case "Ошибки_вида_C": Counter = 5; break;
                            case "Ошибки_вида_D": Counter = 6; break;
                            default: break;
                        }

                        foreach (var XMLFile in XMLShema.Descendants(OurXMLFile.Name))
                        {
                            Found = true;
                        }

                        if (OurXMLFile.Name != "Ошибки_в_XML_файле")
                        {
                            if (Found == false)
                            {
                                Control[Counter] = 0;
                                txtBoxCompareResult.Text = txtBoxCompareResult.Text + "Обнаружена ошибка, в загруженном шаблоне отсутствует элемент \"" + OurXMLFile.Name + "\".\n";
                            }
                            else
                            {
                                Control[Counter] = 1;
                            }
                        }
                        else if (Found == false)
                        {
                            txtBoxCompareResult.Text = txtBoxCompareResult.Text + "В загруженном шаблоне отсутствует корневой элемент \"Ошибки_в_XML_файле\".\n";
                        }
                    }

                    if (OurXMLFile.Name == "Ошибки_вида_D") break;
                }

                string teg = "";

                for (int i = 0; i < 7; i++)
                {
                    Found = false;
                    if (Control[i] == 2)
                    {
                        Control[i] = 0;
                        switch (i)
                        {
                            case 0: teg = "Ошибки_вида_02"; break;
                            case 1: teg = "Ошибки_вида_03"; break;
                            case 2: teg = "Ошибки_вида_04"; break;
                            case 3: teg = "Ошибки_вида_A"; break;
                            case 4: teg = "Ошибки_вида_B"; break;
                            case 5: teg = "Ошибки_вида_C"; break;
                            case 6: teg = "Ошибки_вида_D"; break;
                            default: break;
                        }
                        foreach (var XMLFile in XMLShema.Descendants(teg))
                        {
                            Found = true;
                        }
                        if (Found) txtBoxCompareResult.Text = txtBoxCompareResult.Text + "Обнаружена ошибка, в загруженном шаблоне: элемент \"" + teg + "\" отсутствует в исходном проверочном файле.\n";
                    }
                }

                XElement rootSource = XMLOurShema.Root;
                XElement rootSchema = XMLShema.Root;

                if (Control[0] == 1)
                {
                    compare_type("Ошибки_вида_02", "НачислДох_ВычетНал", "НачислДох", "ВычетНал", rootSource, rootSchema);
                }
                if (Control[1] == 1)
                {
                    compare_type("Ошибки_вида_03", "ИсчислНал", "ПодсчётИсчислНал", "ИсчислНал", rootSource, rootSchema);
                }
                if (Control[2] == 1)
                {
                    compare_type("Ошибки_вида_04", "ИсчислНал_АвансПлат", "ИсчислНал", "АвансПлат", rootSource, rootSchema);
                }
                if (Control[3] == 1)
                {
                    compare_type("Ошибки_вида_A", "СумДата", "ДатаФактДох", "ДатаУдержНал", rootSource, rootSchema);
                }
                if (Control[4] == 1)
                {
                    compare_type("Ошибки_вида_B", "СумДата", "ДатаУдержНал", "СрокПрчслНал", rootSource, rootSchema);
                }
                if (Control[5] == 1)
                {
                    compare_type("Ошибки_вида_C", "СумУдержНал_СтавкаОтСумФактДоход", "СумУдержНал", "СтавкаОтСумФактДоход", rootSource, rootSchema);
                }
                if (Control[6] == 1)
                {
                    compare_type("Ошибки_вида_D", "УдержНалИт_СумУдержНал", "УдержНалИт", "СумУдержНал", rootSource, rootSchema);
                }

                if (String.IsNullOrEmpty(txtBoxCompareResult.Text))
                {
                    txtBoxCompareResult.Text = "Ошибок не обнаружено.";
                }
            }
        }


        // сравнение ошибок блоков  1.2 - 1.4, A - D
        void compare_type(string ElementName, string SubElementName, string AttributeElement1, string AttributeElement2, XElement rootSource, XElement rootSchema)
        {
            string sourceAtr_1;
            string sourceAtr_2;

            string schemaAtr_1;
            string schemaAtr_2;

            List<List<string>> listSource = new List<List<string>>();
            List<string> currLineSource;

            List<List<string>> listSchema = new List<List<string>>();
            List<string> currLineSchema;

            List<int> listDelSource = new List<int>();
            List<int> listDelSchema = new List<int>();

            int ind = 0;

            foreach (var sourceElem in rootSource.Element(ElementName).Descendants(SubElementName))
            {
                sourceAtr_1 = sourceElem.Attribute(AttributeElement1).Value;
                sourceAtr_2 = sourceElem.Attribute(AttributeElement2).Value;

                currLineSource = new List<string>();

                currLineSource.Add(sourceAtr_1);
                currLineSource.Add(sourceAtr_2);
                listSource.Add(currLineSource);
                ind++;
            }

            foreach (var schemaElem in rootSchema.Element(ElementName).Descendants(SubElementName))
            {
                schemaAtr_1 = schemaElem.Attribute(AttributeElement1).Value;
                schemaAtr_2 = schemaElem.Attribute(AttributeElement2).Value;

                currLineSchema = new List<string>();

                currLineSchema.Add(schemaAtr_1);
                currLineSchema.Add(schemaAtr_2);
                listSchema.Add(currLineSchema);

                ind++;
            }

            int i = 0;
            int j = 0;

            currLineSource = new List<string>();

            foreach (var elemSource in listSource)
            {
                j = 0;
                foreach (var elemSchema in listSchema)
                {
                    if ((elemSource[0] == elemSchema[0]) && (elemSource[1] == elemSchema[1]))
                    {
                        listDelSource.Add(i);
                        listDelSchema.Add(j);
                        break;
                    }
                    j++;
                }
                i++;
            }

            for (i = listDelSource.Count - 1; i >= 0; i--)
            {
                listSource.RemoveAt(listDelSource[i]);
            }

            for (j = listDelSchema.Count - 1; j >= 0; j--)
            {
                listSchema.RemoveAt(listDelSchema[j]);
            }


            if (listSource.Count != 0)
            {
                ind = 0;
                do
                {
                    txtBoxCompareResult.Text += "\n" + ElementName + ". Элемент " + SubElementName + ", атрибут " + AttributeElement1 + "=\"" + listSource[ind][0] +
                        "\" атрибут " + AttributeElement2 + "=\"" + listSource[ind][1] + "\" отсутствует в проверяемом файле";
                    ind++;
                } while (listSource.Count != ind);
            }

            if (listSchema.Count != 0)
            {
                ind = 0;
                do
                {
                    txtBoxCompareResult.Text += "\n" + ElementName + ". Элемент " + SubElementName + ", атрибут " + AttributeElement1 + "=\"" + listSchema[ind][0] +
                        "\" атрибут " + AttributeElement2 + "=\"" + listSchema[ind][1] + "\" отсутствует в исходном проверочном файле";
                    ind++;
                } while (listSchema.Count != ind);
            }
        }

        // 1.2. Cравнение начисленных доходов и налоговых вычетов
        void check_unrecorded_revenue_and_tax_deductions(XDocument XMLsource)
        {
            double dblUnrecordedRevenue;        // начисленные доходы, строка 020
            double dblTaxDeductions;            // налоговый вычет, строка 030

            txtBoxErrorsSource.Text += "\nСравнение начисленных доходов и налоговых вычетов:\n";
            
            XElement rootElemDate = new XElement("Ошибки_вида_02");

            foreach (var sum in XMLsource.Descendants("СумСтавка"))
            {
                dblUnrecordedRevenue = double.Parse(sum.Attribute("НачислДох").Value, CultureInfo.InvariantCulture);
                dblTaxDeductions = double.Parse(sum.Attribute("ВычетНал").Value, CultureInfo.InvariantCulture);
                if (dblUnrecordedRevenue < dblTaxDeductions)
                {
                    XElement elemDate = new XElement("НачислДох_ВычетНал",
                            new XAttribute("НачислДох", dblUnrecordedRevenue),
                            new XAttribute("ВычетНал", dblTaxDeductions));
                    rootElemDate.Add(elemDate);

                    txtBoxErrorsSource.Text += "Ошибка атрибута НачислДох\n";
                }
                else
                    txtBoxErrorsSource.Text += "Ошибок нет!\n";
            }

            if (rootElemDate.HasElements)
                totalRootElement.Add(rootElemDate);
        }

        // 1.3. Проверка исчисляемого налога (с учетом соотношения 1.2).
        void check_countable_taxes(XDocument XMLsource)
        {
            double UnrecordedRevenue;        // начисленные доходы, строка 020
            double TaxDeductions;            // налоговый вычет, строка 030
            double Rate;                        // Ставка, строка 010
            double CountableTaxes;              // Исчисляемый налог, строка 040

            txtBoxErrorsSource.Text += "\nПроверка исчисляемого налога:\n";

            XElement rootElemDate = new XElement("Ошибки_вида_03");

            foreach (var sum in XMLsource.Descendants("СумСтавка"))
            {
                UnrecordedRevenue = double.Parse(sum.Attribute("НачислДох").Value, CultureInfo.InvariantCulture);
                TaxDeductions = double.Parse(sum.Attribute("ВычетНал").Value, CultureInfo.InvariantCulture);
                Rate = double.Parse(sum.Attribute("Ставка").Value, CultureInfo.InvariantCulture);
                CountableTaxes = double.Parse(sum.Attribute("ИсчислНал").Value, CultureInfo.InvariantCulture);

                if (UnrecordedRevenue >= TaxDeductions && Rate > 0)
                {
                    if (((UnrecordedRevenue - TaxDeductions) / Rate) != CountableTaxes)
                    {
                        XElement elemDate = new XElement("ИсчислНал",
                                new XAttribute("ПодсчётИсчислНал", Math.Round((UnrecordedRevenue - TaxDeductions) / Rate, 0)),
                                new XAttribute("ИсчислНал", CountableTaxes));
                        rootElemDate.Add(elemDate);

                        txtBoxErrorsSource.Text += "Ошибка атрибута ИсчислНал\n";
                    }
                    else
                        txtBoxErrorsSource.Text += "Ошибок нет!\n";
                }
                else
                    txtBoxErrorsSource.Text += "Невозможно произвести расчёты!\n";
            }

            if (rootElemDate.HasElements)
                totalRootElement.Add(rootElemDate);
        }

        // 1.4. Исчисляемый налог должен быть больше или равен авансовому платёжу (строка 040 >, = строка 050)
        void check_countable_taxes_and_advance_payments(XDocument XMLsource)
        {
            double CountableTaxes;          // Исчисляемый налог
            double AdvancePayments;         // Авансовый платёж

            txtBoxErrorsSource.Text += "\nСравнение результатов исчисляемого налога и авансовых платежей:\n";

            XElement rootElemDate = new XElement("Ошибки_вида_04");

            foreach (var SumBet in XMLsource.Descendants("СумСтавка"))
            {
                CountableTaxes = double.Parse(SumBet.Attribute("ИсчислНал").Value);    // Исчисляемый налог
                AdvancePayments = double.Parse(SumBet.Attribute("АвансПлат").Value);    // Авансовый платёж

                if (CountableTaxes < AdvancePayments)
                {
                    XElement elemDate = new XElement("ИсчислНал_АвансПлат",
                        new XAttribute("ИсчислНал", CountableTaxes),
                        new XAttribute("АвансПлат", AdvancePayments));
                    rootElemDate.Add(elemDate);

                    txtBoxErrorsSource.Text += "Ошибка элемента СумСтавка атрибута ИсчислНал со значением " + CountableTaxes +
                           " и атрибута АвансПлат со значением " + AdvancePayments + "\n";
                }
            }

            if (rootElemDate.HasElements)
                totalRootElement.Add(rootElemDate);
        }

        // A. Cравнение дат получения дохода и удержания налога
        void check_income_earning_and_tax_deduction(XDocument XMLsource)
        {
            DateTime dateIncomeEarning;     // получение дохода
            DateTime dateTaxDeduction;      // удержание налога (налоговый вычет)
            int comparisonOfTheResults;     // сравнение результатов
            string strIncomeEarning;
            string strTaxDeduction;

            txtBoxErrorsSource.Text += "\nСравнение результатов дат:\n";

            XElement rootElemDate = new XElement("Ошибки_вида_A");

            foreach (var sumDate in XMLsource.Descendants("СумДата"))
            {

                dateIncomeEarning = DateTime.Parse(sumDate.Attribute("ДатаФактДох").Value);
                dateTaxDeduction = DateTime.Parse(sumDate.Attribute("ДатаУдержНал").Value);

                comparisonOfTheResults = DateTime.Compare(dateIncomeEarning, dateTaxDeduction);

                if (comparisonOfTheResults > 0)
                {
                    strIncomeEarning = dateIncomeEarning.ToString("dd:MM:yyy");
                    strTaxDeduction = dateTaxDeduction.ToString("dd:MM:yyy");

                    XElement elemDate = new XElement("СумДата",
                        new XAttribute("ДатаФактДох", strIncomeEarning),
                        new XAttribute("ДатаУдержНал", strTaxDeduction));
                    rootElemDate.Add(elemDate);

                    txtBoxErrorsSource.Text += "Ошибка элемента СумДата атрибута ДатаФактДох со значением " + strIncomeEarning +
                       " и атрибута ДатаУдержНал со значением " + strTaxDeduction + "\n";
                }
            }

            if (rootElemDate.HasElements)
                totalRootElement.Add(rootElemDate);

        }

        // B. Cравнение срока перечисления и даты удержания налога
        void check_date_of_transfer_and_tax_deduction(XDocument XMLsource)
        {
            DateTime dateOfTransfer; // срок перечисления
            DateTime dateTaxDeduction; // удержание налога (налоговый вычет)
            int comparisonOfTheResults; // сравнение результатов
            string strDateOfTransfer;
            string strTaxDeduction;

            txtBoxErrorsSource.Text += "\nСравнение результатов дат:\n";

            XElement rootElemDate = new XElement("Ошибки_вида_B");

            foreach (var sumDate in XMLsource.Descendants("СумДата"))
            {
                dateOfTransfer = DateTime.Parse(sumDate.Attribute("СрокПрчслНал").Value);
                dateTaxDeduction = DateTime.Parse(sumDate.Attribute("ДатаУдержНал").Value);

                comparisonOfTheResults = DateTime.Compare(dateTaxDeduction, dateOfTransfer);

                if (comparisonOfTheResults > 0)
                {
                    strDateOfTransfer = dateOfTransfer.ToString("dd:MM:yyy");
                    strTaxDeduction = dateTaxDeduction.ToString("dd:MM:yyy");

                    XElement elemDate = new XElement("СумДата",
                    new XAttribute("ДатаУдержНал", strTaxDeduction),
                    new XAttribute("СрокПрчслНал", strDateOfTransfer));
                    rootElemDate.Add(elemDate);

                    txtBoxErrorsSource.Text += "Ошибка элемента СумДата атрибута СрокПрчслНал со значением " + strDateOfTransfer +
                    " и атрибута ДатаУдержНал со значением " + strTaxDeduction + "\n";
                }
            }

            if (rootElemDate.HasElements)
                totalRootElement.Add(rootElemDate);
        }

        // C. Сумма удержанного налога не должна превышать 13% от суммы фактически полученного дохода (налог округляется до 1 руб.)
        void check_sum_withheld_tax_and_sum_actually_received_income(XDocument XMLsource)
        {
            double SumWithheldTax = 0;                  // Сумма удержанного налога
            double SumActuallyReceivedIncome = 0;       // Сумма фактически полученного дохода 
            double PercentActuallyReceivedIncome = 0;   // 13% от суммы фактически полученного дохода

            txtBoxErrorsSource.Text += "\nСравнение результатов суммы удержанного налога и суммы фактически полученного дохода:\n";

            XElement rootElemDate = new XElement("Ошибки_вида_C");

            foreach (var sumDate in XMLsource.Descendants("СумДата"))
            {
                SumWithheldTax = SumWithheldTax + Math.Round(double.Parse(sumDate.Attribute("УдержНал").Value), 0);     //Сумма удержанного налога, где налог округляется до 1 руб.
                SumActuallyReceivedIncome = SumActuallyReceivedIncome +
                    double.Parse(sumDate.Attribute("ФактДоход").Value, CultureInfo.InvariantCulture);                   //Сумма фактически полученного дохода
            }

            PercentActuallyReceivedIncome = SumActuallyReceivedIncome * 0.13;       // 13% от суммы фактически полученного дохода

            if (PercentActuallyReceivedIncome < SumWithheldTax)
            {
                XElement elemDate = new XElement("СумУдержНал_СтавкаОтСумФактДоход",
                        new XAttribute("СумУдержНал", SumWithheldTax),
                        new XAttribute("СтавкаОтСумФактДоход", Math.Round(PercentActuallyReceivedIncome, 0)));
                rootElemDate.Add(elemDate);

                txtBoxErrorsSource.Text += "Ошибка элементов СумДата атрибутов ФактДоход со ставкой 13% со значением суммы " + Math.Round(PercentActuallyReceivedIncome, 0) +
                       " и атрибутов УдержНал со значением суммы " + SumWithheldTax + "\n";
            }

            if (rootElemDate.HasElements)
                totalRootElement.Add(rootElemDate);
        }

        // D. Общая сумма удержанного налога Раздела 2 не должна превышать сумму удержанного налога Разела 1.
        void check_sum_withheld_tax_section1_and_section2(XDocument XMLsource)
        {
            double SumWithheldTax1 = 0;       // Сумма удержанного налога раздела 1
            double SumWithheldTax2 = 0;       // Сумма удержанного налога раздела 2

            txtBoxErrorsSource.Text += "\nСравнение результатов суммы удержанного налога раздела 1 и суммы удержанного налога раздела 2:\n";

            XElement rootElemDate = new XElement("Ошибки_вида_D");

            foreach (var GeneralizingShow in XMLsource.Descendants("ОбобщПоказ"))
            {
                SumWithheldTax1 = double.Parse(GeneralizingShow.Attribute("УдержНалИт").Value);    //Сумма удержанного налога раздела 1.
            }

            foreach (var sumDate in XMLsource.Descendants("СумДата"))
            {
                SumWithheldTax2 = SumWithheldTax2 + double.Parse(sumDate.Attribute("УдержНал").Value);    //Сумма удержанного налога раздела 2.
            }

            if (SumWithheldTax1 < SumWithheldTax2)
            {
                XElement elemDate = new XElement("УдержНалИт_СумУдержНал",
                        new XAttribute("УдержНалИт", SumWithheldTax1),
                        new XAttribute("СумУдержНал", SumWithheldTax2));
                rootElemDate.Add(elemDate);

                txtBoxErrorsSource.Text += "Ошибка атрибутов УдержНал элементов СумДата со значением суммы " + SumWithheldTax2 +
                       " и атрибута УдержНалИт элемента ОбобщПоказ со значением суммы " + SumWithheldTax1 + "\n";
            }

            if (rootElemDate.HasElements)
                totalRootElement.Add(rootElemDate);
        }

        string read_xml_from_file()
        {
            Nullable<bool> result;
            OpenFileDialog dlgOpen;

            dlgOpen = new OpenFileDialog();
            dlgOpen.Title = "Выберите файл";
            dlgOpen.FileName = "Документ";
            dlgOpen.Filter = "Язык разметки (.xml)|*.xml";
            dlgOpen.FilterIndex = 0;

            result = dlgOpen.ShowDialog();

            if (true == result)
                return dlgOpen.FileName;

            return null;
        }

        string save_xml_in_file()
        {
            Nullable<bool> result;
            SaveFileDialog dlgSave;

            dlgSave = new SaveFileDialog();
            dlgSave.Title = "Выберите файл";
            dlgSave.FileName = "Документ";
            dlgSave.Filter = "Язык разметки (.xml)|*.xml";
            dlgSave.FilterIndex = 0;

            result = dlgSave.ShowDialog();

            if (true == result)
                return dlgSave.FileName;

            return null;
        }


        private void AboutProgram_Click(object sender, RoutedEventArgs e)
        {
            string Header = "О программе";
            string Content = "Данное приложение служит для форматно-логического контроля xml-файла регламентированной отчетности 6-НДФЛ.";
            MessageBox.Show(Content, Header, MessageBoxButton.OK, MessageBoxImage.Information);

        }

        private void AboutAuthor_Click(object sender, RoutedEventArgs e)
        {
            string Header = "Об авторах";
            string Content = "Авторы приложения: \n" +
                             " - Большаков Антон Александрович\n" + 
                             " - Поярков Игорь Евгеньевич";
            MessageBox.Show(Content, Header, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ExitToProgram_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
