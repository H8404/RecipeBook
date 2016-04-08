using System;
using System.Collections.Generic;
using System.IO;
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
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;


namespace RecipeBook
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        XmlDocument doc = new XmlDocument();
        XDocument xdoc = new XDocument();
        List<string> ingredients = new List<string>();
        List<string> recipeText = new List<string>();
        bool modifyText;
        int previousIndex;

        public MainWindow()
        {
            InitializeComponent();
            Init();
        }

        private void Init()
        {
            if (File.Exists("recipeXML.xml") == true)
            {
                RefreshComboBox();
                RefreshListBox();
            }
        }

        //RESEPTIEN KATSELU SIVU
        private void lbReipes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lbRecipes.SelectedIndex >= 0)
            {
                try
                {
                    ReadRecipe();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void btnShowAll_Click(object sender, RoutedEventArgs e)
        {
            txtSearchFromXML.Text = "";
            cbGategory.SelectedIndex = -1;
            RefreshListBox();
        }

        private void btnSearchFromXML_Click(object sender, RoutedEventArgs e)
        {
            var xdoc = XDocument.Load("recipeXML.xml");
            string searchWord = txtSearchFromXML.Text;
            var recipes = from cookbook in xdoc.Descendants("Recipe")
                          let nameEle = cookbook.Element("Title")
                          where string.Equals(nameEle.Value, searchWord, StringComparison.OrdinalIgnoreCase)
                          select new
                          {
                              Title = cookbook.Element("Title").Value,
                              id = cookbook.Attribute("id").Value
                          };
            if (recipes.Count() > 0)
            {
                lbRecipes.ItemsSource = recipes;
                lbRecipes.SelectedValuePath = "id";
            }
            else
            {
                MessageBox.Show(string.Format("Hakusanalla {0} ei löytynyt reseptiä!", searchWord));
            }
        }

        private void cbGategory_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbGategory.SelectedIndex >= 0)
            {
                var xdoc = XDocument.Load("recipeXML.xml");
                string category = cbGategory.SelectedValue.ToString();
                var recipes = from cookbook in xdoc.Descendants("Recipe")
                              where cookbook.Element("Category").Value == category
                              select new
                              {
                                  Title = cookbook.Element("Title").Value,
                                  id = cookbook.Attribute("id").Value
                              };
                lbRecipes.ItemsSource = recipes;
                lbRecipes.SelectedValuePath = "id";
            }
        }

        //RESEPTIEN LISÄYS/MUOKKAUS SIVU
        private void btnAddIngredient_Click(object sender, RoutedEventArgs e)
        {
            if (btnAddIngredient.Content.ToString() == "Lisää")
            {
                lvAddIngredient.Visibility = Visibility.Visible;
                string ingredient = txtIngredient.Text;
                lvAddIngredient.Items.Add(ingredient);
                txtIngredient.Text = "";
            }
            else
            {
                modifyText = true;
                int index = lvAddIngredient.SelectedIndex;
                lvAddIngredient.Items[index] = txtIngredient.Text;
                txtIngredient.Text = "";
            }
            modifyText = false;
        }

        private void btnAddRecipeText_Click(object sender, RoutedEventArgs e)
        {
            if (btnAddIngredient.Content.ToString() == "Lisää")
            {
                lvAddRecipeText.Visibility = Visibility.Visible;
                string recipeText = txtRecipeText.Text;
                lvAddRecipeText.Items.Add(recipeText);
                txtRecipeText.Text = "";
            }
            else
            {
                modifyText = true;
                int index = lvAddRecipeText.SelectedIndex;
                lvAddRecipeText.Items[index] = txtRecipeText.Text;
                txtRecipeText.Text = "";
            }
            modifyText = false;
        }

        private void btnSaveToXML_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in lvAddIngredient.Items)
            {
                string text = item.ToString();
                ingredients.Add(text);
            }
            foreach (var item in lvAddRecipeText.Items)
            {
                string text = item.ToString();
                recipeText.Add(text);
            }
            if (btnSaveToXML.Content.ToString() == "Tallenna resepti")
            {
                if (File.Exists("recipeXML.xml") == false)
                {
                    try
                    {
                        CreateXMLDoc();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                    finally
                    {
                        MessageBox.Show("Resepti lisätty onnistuneesti");
                        RefreshComboBox();
                        ClearAddRecipe();
                        RefreshListBox();
                        ingredients.Clear();
                        recipeText.Clear();
                    }
                }
                else
                {
                    try
                    {
                        AddXMLElements();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                    finally
                    {
                        MessageBox.Show("Resepti lisätty onnistuneesti");
                        RefreshComboBox();
                        ClearAddRecipe();
                        RefreshListBox();
                        ingredients.Clear();
                        recipeText.Clear();
                    }
                }
            }
            else
            {
                try
                {
                    UpdateRecipe();
                }
                catch (Exception ex)
                {

                    MessageBox.Show(ex.Message);
                }
                finally
                {
                    RefreshComboBox();
                    RefreshListBox();
                    ClearAddRecipe();
                    lvAddIngredient.SelectedIndex = -1;
                    lvAddRecipeText.SelectedIndex = -1;
                    tabAddRecipe.Header = "Lisää resepti";
                    btnAddIngredient.Content = "Lisää";
                    btnAddRecipeText.Content = "Lisää";
                    btnSaveToXML.Content = "Tallenna resepti";
                    btnBackTo.Content = "Tyhjennä lomake";
                    tabView.IsSelected = true;
                    lbRecipes.SelectedIndex = previousIndex;
                    ingredients.Clear();
                    recipeText.Clear();
                    ReadRecipe();
                }
            }
        }

        private void lvAddIngredient_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (modifyText == false && lvAddIngredient.SelectedIndex >= 0)
            {
                txtIngredient.Text = lvAddIngredient.SelectedItem.ToString();
            }
        }

        private void lvAddRecipeText_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (modifyText == false && lvAddRecipeText.SelectedIndex >= 0)
            {
                txtRecipeText.Text = lvAddRecipeText.SelectedItem.ToString();
            }
        }

        private void btnBackTo_Click(object sender, RoutedEventArgs e)
        {
            if (btnBackTo.Content.ToString() == "Tyhjennä lomake")
            {
                ClearAddRecipe();
            }
            else
            {
                ClearAddRecipe();
                tabView.IsSelected = true;
                tabAddRecipe.Header = "Lisää resepti";
                btnBackTo.Content = "Tyhjennä lomake";
                btnAddIngredient.Content = "Lisää";
                btnAddRecipeText.Content = "Lisää";
                btnSaveToXML.Content = "Tallenna resepti";
            }
        }

        //RESEPTIN KATSELU SIVU
        private void btnModifyRecipe_Click(object sender, RoutedEventArgs e)
        {
            lvAddRecipeText.Visibility = Visibility.Visible;
            lvAddIngredient.Visibility = Visibility.Visible;
            btnSaveToXML.Content = "Tallenna muutokset";
            btnAddIngredient.Content = "OK";
            btnAddRecipeText.Content = "OK";
            string id = lbRecipes.SelectedValue.ToString();
            string search = "/CookBook/Recipe[@id='" + id + "']";
            XmlNodeList list = doc.SelectNodes(search);
            tabAddRecipe.IsSelected = true;
            btnBackTo.Content = "Peruuta";
            tabAddRecipe.Header = "Muokkaa Reseptiä";
            string search2 = "/CookBook/Recipe[@id='" + id + "']/Ingredient/li";
            string search3 = "/CookBook/Recipe[@id='" + id + "']/RecipeText/li";
            XmlNodeList list2 = doc.SelectNodes(search2);
            XmlNodeList list3 = doc.SelectNodes(search3);
            foreach (XmlNode xn in list)
            {
                txtTitle.Text = xn["Title"].InnerText;
                txtAmount.Text = xn["Amount"].InnerText;
                cbAddCategory.Text = xn["Category"].InnerText;
                txtDescription.Text = xn["Description"].InnerText;
            }
            foreach (XmlNode xn in list2)
            {
                string listmember = xn.InnerText;
                lvAddIngredient.Items.Add(listmember);
            }
            foreach (XmlNode xn in list3)
            {
                string listmember = xn.InnerText;
                lvAddRecipeText.Items.Add(listmember);
            }

        }

        private void btnDeleteRecipe_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string id = lbRecipes.SelectedValue.ToString();
                string search = "/CookBook/Recipe[@id='" + id + "']";
                XmlNode node = doc.SelectSingleNode(search);
                if (node != null)
                {
                    var result = MessageBox.Show("Haluatko varmasti poistaa reseptin ?", "Poista resepti", MessageBoxButton.YesNo);
                    if (result == MessageBoxResult.Yes)
                    {
                        node.ParentNode.RemoveChild(node);
                        doc.Save("recipeXML.xml");
                    }
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
            finally
            {
                tbHeading.Text = "";
                tbAmount.Text = "";
                tbDescription.Text = "";
                icIngredient.Items.Clear();
                icRecipeText.Items.Clear();
                tabRecipe.IsSelected = true;
                tabView.Visibility = Visibility.Hidden;
                lbRecipes.SelectedIndex = -1;
                RefreshComboBox();
                RefreshListBox();
            }

        }

        //METODIT
        private void ReadRecipe()
        {
            tabView.Visibility = Visibility.Visible;
            icIngredient.Items.Clear();
            icRecipeText.Items.Clear();
            string id = lbRecipes.SelectedValue.ToString();
            string search = "/CookBook/Recipe[@id='" + id + "']";
            string search2 = "/CookBook/Recipe[@id='" + id + "']/Ingredient/li";
            string search3 = "/CookBook/Recipe[@id='" + id + "']/RecipeText/li";
            XmlNodeList list = doc.SelectNodes(search);
            XmlNodeList list2 = doc.SelectNodes(search2);
            XmlNodeList list3 = doc.SelectNodes(search3);
            foreach (XmlNode xn in list)
            {
                tabView.Header = xn["Title"].InnerText;
                tbHeading.Text = xn["Title"].InnerText;
                tbAmount.Text = xn["Amount"].InnerText;
                tbDescription.Text = xn["Description"].InnerText;
            }

            foreach (XmlNode node in list2)
            {
                string listmember = node.InnerText;
                icIngredient.Items.Add(listmember);
            }

            foreach (XmlNode node in list3)
            {
                string listmember = node.InnerText;
                icRecipeText.Items.Add(listmember);
            }
        }

        private void UpdateRecipe()
        {
            previousIndex = lbRecipes.SelectedIndex;
            string id = lbRecipes.SelectedValue.ToString();
            string search = "/CookBook/Recipe[@id='" + id + "']";
            string search2 = "/CookBook/Recipe[@id='" + id + "']/Ingredient/li";
            string search3 = "/CookBook/Recipe[@id='" + id + "']/RecipeText/li";
            XmlNodeList list = doc.SelectNodes(search);
            XmlNodeList list2 = doc.SelectNodes(search2);
            XmlNodeList list3 = doc.SelectNodes(search3);
            foreach (XmlNode node in list)
            {
                node["Title"].InnerText = txtTitle.Text;
                node["Category"].InnerText = cbAddCategory.Text;
                node["Amount"].InnerText = txtAmount.Text;
                node["Description"].InnerText = txtDescription.Text;
            }

            for (int i = 0; i < ingredients.Count; i++)
            {
                list2[i].InnerText = ingredients[i].ToString();
            }

            for (int i = 0; i < recipeText.Count; i++)
            {
                list3[i].InnerText = recipeText[i].ToString();
            }

            doc.Save("recipeXML.xml");

        }

        private void CreateXMLDoc()
        {
            string selectedValue = cbAddCategory.Text;
            var newElement = new XElement("CookBook",
                new XElement("Recipe", new XAttribute("id", "1"),
                new XElement("Title", txtTitle.Text),
                new XElement("Category", selectedValue.ToString()),
                new XElement("Description", txtDescription.Text),
                new XElement("Amount", txtAmount.Text),
                new XElement("Ingredient", ingredients.Select(text => new XElement("li", text))),
                new XElement("RecipeText", recipeText.Select(text => new XElement("li", text)))));
            xdoc.Add(newElement);
            xdoc.Save("recipeXML.xml");
        }

        private void AddXMLElements()
        {
            previousIndex = lbRecipes.SelectedIndex;
            int lastItemIndex = lbRecipes.Items.Count - 1;
            lbRecipes.SelectedIndex = lastItemIndex;
            string currentId = lbRecipes.SelectedValue.ToString();
            int id = Int32.Parse(currentId) + 1;
            string selectedCategory = cbAddCategory.Text;
            var xdoc = XDocument.Load("recipeXML.xml");
            var newElement = new XElement("Recipe", new XAttribute("id", id.ToString()),
                new XElement("Title", txtTitle.Text),
                new XElement("Category", selectedCategory.ToString()),
                new XElement("Description", txtDescription.Text),
                new XElement("Amount", txtAmount.Text),
                new XElement("Ingredient", ingredients.Select(text => new XElement("li", text))),
                new XElement("RecipeText", recipeText.Select(text => new XElement("li", text))));
            xdoc.Element("CookBook").Add(newElement);
            xdoc.Save("recipeXML.xml");
            lbRecipes.SelectedIndex = previousIndex;
            if (previousIndex == -1)
            {
                tabView.Visibility = Visibility.Hidden;
            }
        }

        private void ClearAddRecipe()
        {
            txtTitle.Text = "";
            cbAddCategory.SelectedIndex = -1;
            txtAmount.Text = "";
            txtDescription.Text = "";
            txtIngredient.Text = "";
            txtRecipeText.Text = "";
            lvAddIngredient.Items.Clear();
            lvAddRecipeText.Items.Clear();
            lvAddIngredient.Visibility = Visibility.Hidden;
            lvAddRecipeText.Visibility = Visibility.Hidden;
        }

        private void RefreshListBox()
        {
            /*(FindResource("RecipeData") as XmlDataProvider).Refresh();*/
            doc.Load("recipeXML.xml");
            var xdoc = XDocument.Load("recipeXML.xml");
            var recipes = from cookbook in xdoc.Descendants("Recipe")
                          select new
                          {
                              Title = cookbook.Element("Title").Value,
                              id = cookbook.Attribute("id").Value
                          };
            lbRecipes.ItemsSource = recipes;
            lbRecipes.SelectedValuePath = "id";
        }

        private void RefreshComboBox()
        {
            doc.Load("recipeXML.xml");
            XmlNodeList categoryXML = doc.SelectNodes("/CookBook/Recipe/Category");
            List<string> categoryList = new List<string>();
            for (int i = 0; i < categoryXML.Count; i++)
            {
                categoryList.Add(categoryXML[i].InnerText);
            }
            categoryList = categoryList.Distinct().ToList();
            cbGategory.ItemsSource = categoryList;
            cbGategory.SelectedIndex = -1;
        }
    }
}
