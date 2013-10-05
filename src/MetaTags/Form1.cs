using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading.Tasks;

namespace MetaTags
{
    public partial class Form1 : Form
    {
        private List<Tuple<string, string>> Attributes = new List<Tuple<string, string>>();

        private WebBrowser Web = null;

        private enum TypeParameter { Good, Nothing, Error}

        public Form1()
        {
            InitializeComponent();
            StartNewBrowser();
        }

        private bool Navigate(String address)
        {
            if (String.IsNullOrEmpty(address)) return false;
            if (address.Equals("about:blank")) return false;
            if (!address.StartsWith("http://") &&
                !address.StartsWith("https://"))
            {
                address = "http://" + address;
            }

            try
            {
                Web.Navigate(new Uri(address));
                return true;
            }
            catch (System.UriFormatException err)
            {
                throw (new Exception("Ошибка в uri. \n" + err.Message));
            }
            catch (Exception err)
            {
                throw(new Exception("Ошибка при переходе к документу. \n" + err.Message));
                //return false;
            }
        }

        private void getAttribute()
        {
            try
            {
                HtmlElementCollection elems = Web.Document.GetElementsByTagName("META");
                if (elems != null)
                {
                    foreach (HtmlElement elem in elems)
                    {
                        String nameStr = elem.GetAttribute("NAME");
                        if (nameStr != null && nameStr.Length != 0)
                        {
                            String contentStr = elem.GetAttribute("CONTENT");
                            Attributes.Add(new Tuple<string, string>(nameStr, contentStr));
                        }
                    }
                }
            }
            catch (Exception err)
            {
                throw (new Exception("Ошибка во время париснга документа. \n" + err.Message));
            }
        }

        private void WriteInfo(string param)
        {
            textBox1.Clear();

            if (Attributes.Count == 0 || Address.Text == "")
            {
                MessageBox.Show("Страница не загружена или нет данных для отображения.",
                                "Информационное сообщение",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            
            if (param != "")
            {
                Parallel.ForEach(Attributes,
                (curValue, loopstate) =>
                {
                    if (param.ToLower() == curValue.Item1.ToLower())
                    {
                        textBox1.Text = curValue.Item2;
                        loopstate.Stop();
                        return;
                    }
                });
                
                /*foreach (var el in Attributes)
                {
                    if (param.ToLower() == el.Item1.ToLower())
                    {
                        textBox1.Text = el.Item2;
                        return;
                    }
                }*/
            }
        }

        private void Manage()
        {
            if (Address.Text == toolStripTextBox1.Text && Address.Text != "")
            {
                MessageBox.Show("Информация о сайте " + Address.Text +
                                " уже загружена.", "Информационное сообщение",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (toolStripTextBox1.Text == "")
            {
                MessageBox.Show("Указан пустой url.", "Информационное сообщение",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            Clear();

            //StartNewBrowser();

            try
            {
                progressBar1.Visible = true;

                if (Navigate(toolStripTextBox1.Text) != true)
                    throw (new Exception("Не корректный url."));
                
                while (Web.ReadyState != WebBrowserReadyState.Complete)
                    Application.DoEvents();

                getAttribute();
                    
                if(Attributes.Count == 0)
                    throw (new Exception("Не удалось загрузить указанный сайт."));

                Address.Text = toolStripTextBox1.Text;
                progressBar1.Visible = false;

                MessageBox.Show("Информация о сайте " + Address.Text +
                                " загружена.", "Информационное сообщение",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);

                WriteInfo(comboBox1.Text);
                        
            }
            catch (Exception err)
            {
                progressBar1.Visible = false;
                MessageBox.Show("Произошла ошибка во время работы программы: \n\n" + 
                                err.Message, "Ошибка", 
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                progressBar1.Visible = false;
            }
        }

        private void StartNewBrowser()
        {
            ClearWebBrowser();

            Web = new WebBrowser();
            Web.ScriptErrorsSuppressed = true;
            Web.Visible = false;
        }

        private void ClearWebBrowser()
        {
            if(Web != null)
                Web.Dispose();
            Web = null;
        }

        private void Clear()
        {
            Attributes.Clear();
            Address.Text = "";
            textBox1.Clear();
        }

        private void exitButton_Click(object sender, EventArgs e)
        {
            ClearAll();
            Close();
        }

        private void ClearAll()
        {
            progressBar1.Visible = false;
            ClearWebBrowser();
            toolStripTextBox1.Clear();
            comboBox1.SelectedIndex = -1;
            Clear();
        }

        private void goButton_Click(object sender, EventArgs e)
        {
            Manage();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Attributes.Count != 0)
            {
                try
                {
                    progressBar1.Visible = true;
                    WriteInfo(comboBox1.Text);
                    progressBar1.Visible = false;
                }
                catch (Exception err)
                {
                    progressBar1.Visible = false;
                    MessageBox.Show("Произошла ошибка во время поиска содержимого аттрибута. \n\n" +
                                err.Message, "Ошибка",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);

                }
                finally
                {
                    progressBar1.Visible = false;
                }
            }
        }

        private void toolStripTextBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                Manage();
            }
        }
    }
}
