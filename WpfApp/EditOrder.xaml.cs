using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Diagnostics;
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
using System.Windows.Shapes;

namespace WpfApp
{
    /// <summary>
    /// Interaction logic for EditOrder.xaml
    /// </summary>
    public partial class EditOrder : Window
    {

        public int orders_id_temp_EditOrder { get; set; }
        public int list_of_order_items_temp_EditOrder { get; set; }

        // Хранение значения "Id" при редактировании для SaveProductInOrder_Button_Click
        int product_id_currentValue { get; set; }
        // Хранение значения "Названия товара" при редактировании для SaveProductInOrder_Button_Click
        string nameProduct_currentValue { get; set; }
        // Хранение значения "Количество товара" при редактировании для SaveProductInOrder_Button_Click
        int countProduct__currentValue { get; set; }
        //int curruntRow { get; set; }

        ObservableCollection<Item> col;
        public EditOrder()
        {
            InitializeComponent();

            col = new ObservableCollection<Item>();            
        }

        // Загрузка начальных значений в окно EditOrder
        public void LoadDataInEditOrder(object sender, RoutedEventArgs e)
        {
            //MessageBox.Show(this.orders_id_temp_EditOrder.ToString());
            try
            {                 
                using (OrdersdbEntities db = new OrdersdbEntities())
                {
                    TextBox_NameClient.Text = db.Orders.Where(x => x.orders_id == orders_id_temp_EditOrder)
                        .Select(x => x.name_client).First();
                   
                    comboBox_nameProduct.Items.Add("Не выбрано");
                    var product = db.Products;                 
                    foreach (Product u in product)
                        comboBox_nameProduct.Items.Add(u.product_name);

                    comboBox_nameProduct.SelectedIndex = 0;

                    TextBox_count.Text = "0";

                    var dataFromQuary = from o in db.Orders.AsEnumerable()
                                        from l in db.List_of_order_items
                                        from p in db.Products
                                        where o.orders_id == Convert.ToInt32(orders_id_temp_EditOrder)
                                        where l.list_of_order_items__id == o.list_of_order_items
                                        where l.item_product_id == p.product_id
                                        select new
                                        {
                                            product_id = p.product_id,
                                            product_name = p.product_name,
                                            item_count = l.item_count,
                                            //product_price = p.product_price
                                        };
                    foreach (var u in dataFromQuary)
                    {
                        //dataGrid_EditOrder.Items.Add(new Item() { product_id = u.product_id, product_name = u.product_name, item_count = u.item_count1.ToString() });
                        col.Add(new Item() { product_id = u.product_id, product_name = u.product_name, item_count = u.item_count.ToString() });
                    }
                    dataGrid_EditOrder.ItemsSource = col;
                }
            }
            catch (Exception e2)
            {
                MessageBox.Show(e2.Message, "LoadDataInEditOrder", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddProduct1_Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Определение product_id по comboBox_nameProduct
                int product_id_temp = 0;
                using (OrdersdbEntities db = new OrdersdbEntities())
                {
                    product_id_temp = db.Products.Where(x => x.product_name == comboBox_nameProduct.SelectedValue.ToString()).Select(x => x.product_id).First();
                }

                if (comboBox_nameProduct.SelectedValue.ToString() != "Не выбрано" && Convert.ToInt32(TextBox_count.Text) > 0)
                {
                    //dataGrid_EditOrder.Items.Add(new Item() { product_id = product_id_temp, product_name = comboBox_nameProduct.SelectedValue.ToString(), item_count = TextBox_count.Text });
                    col.Add(new Item() { product_id = product_id_temp, product_name = comboBox_nameProduct.SelectedValue.ToString(), item_count = TextBox_count.Text });
                    dataGrid_EditOrder.ItemsSource = col;
                }
                else
                    MessageBox.Show("Выберите товар или введите количество товаров больше 0", "Предупреждение");
            }
            catch (Exception e1)
            {
                MessageBox.Show(e1.Message, "Exception", MessageBoxButton.OK, MessageBoxImage.Error);
                //MessageBox.Show("Выберите товар или введите количество товаров больше 0", "Предупреждение");
            }
        }

        private void DeleteProduct1_Button_Click(object sender, RoutedEventArgs e)
        {
            var index = dataGrid_EditOrder.SelectedIndex;

            // Проверка на то, что была выделена ячейка
            if (index > -1)
            {
                if (MessageBox.Show("Вы действительно хотите удалить запись?", "Предупреждение", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    //dataGrid_EditOrder.Items.RemoveAt(index);

                    // remove the old information
                    Item item_Old = dataGrid_EditOrder.SelectedItem as Item;
                    col.Remove(item_Old);
                    dataGrid_EditOrder.ItemsSource = col;
                }
            }
        }

        private void SaveEditOrder_Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (OrdersdbEntities db = new OrdersdbEntities())
                {                    
                    // 1. Внесение изменений в таблицу Order
                    if (TextBox_NameClient.Text != "")
                    {
                        var nameClient = db.Orders
                            .Where(x => x.orders_id == orders_id_temp_EditOrder)
                            .FirstOrDefault();

                        nameClient.name_client = TextBox_NameClient.Text;
                        db.SaveChanges();

                        // 2. Внесение изменений в таблицу List_of_order_items
                        // Удаление существующих записей с ID: list_of_order_items_temp_EditOrder

                         //   var list_of_order_items_temp = db.List_of_order_items
                         //       .Where(x => x.list_of_order_items__id == list_of_order_items_temp_EditOrder)
                         //       .FirstOrDefault();
                         //db.List_of_order_items.Remove(list_of_order_items_temp);
                        //db.SaveChanges();
                        db.Database.ExecuteSqlCommand("DELETE FROM [dbo].[List_of_order_items] WHERE [list_of_order_items__id] = {0}", new object[] { list_of_order_items_temp_EditOrder }); 


                        // Выборка данных из List_of_order_items и запись в таблицу List_of_order_items                        

                        //foreach (var item in dataGrid_EditOrder.Items.OfType<Item>())
                        //{
                        //    List_of_order_items list_of_order_items = new List_of_order_items();
                        //    list_of_order_items.list_of_order_items__id = list_of_order_items_temp_EditOrder;
                        //    list_of_order_items.item_product_id = item.product_id;
                        //    list_of_order_items.item_count = Convert.ToInt32(item.item_count);

                        //    db.List_of_order_items.Add(list_of_order_items);
                        //}

                        foreach (var u in col)
                        {
                            //dataGrid_EditOrder.Items.Add(new Item() { product_id = u.product_id, product_name = u.product_name, item_count = u.item_count1.ToString() });
                            List_of_order_items list_of_order_items = new List_of_order_items();
                            list_of_order_items.list_of_order_items__id = list_of_order_items_temp_EditOrder;
                            list_of_order_items.item_product_id = db.Products.Where(x => x.product_name == u.product_name).Select(x => x.product_id).FirstOrDefault();
                            list_of_order_items.item_count = Convert.ToInt32(u.item_count);
                            db.List_of_order_items.Add(list_of_order_items);
                        }

                        db.SaveChanges();


                        // Закрытие окна
                        MessageBox.Show("Данные успешно изменены", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("Поле \"ФИО клиента\" пустое", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception e1)
            {
                MessageBox.Show(e1.Message, "SaveEditOrder_Button_Click", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        private void EditProductInOrder_Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var index = dataGrid_EditOrder.SelectedIndex;
                // Проверка на то, что была выделена ячейка
                if (index > -1)
                {
                    using (OrdersdbEntities db = new OrdersdbEntities())
                    {
                        // При выборе строки получение ID заказа
                        object item = dataGrid_EditOrder.SelectedItem;

                        string nameProduct = (dataGrid_EditOrder.SelectedCells[1].Column.GetCellContent(item) as TextBlock).Text.ToString();
                        TextBox_count.Text = (dataGrid_EditOrder.SelectedCells[2].Column.GetCellContent(item) as TextBlock).Text.ToString();

                        //MessageBox.Show(nameProduct);
                        comboBox_nameProduct.SelectedItem = nameProduct;
                    }
                }


            }
            catch (Exception e1)
            {
                MessageBox.Show(e1.Message, "EditProductInOrder_Button_Click", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        private void SaveProductInOrder_Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (OrdersdbEntities db = new OrdersdbEntities())
                {
                    if (dataGrid_EditOrder.SelectedIndex >= 0)
                    {
                        //MessageBox.Show("SelectedIndex");

                        // get the selected's infomation first
                        Item item_New = new Item();
                        //Item item = dataGrid_EditOrder.SelectedItem as Item;
                        //MessageBox.Show(" " + item.product_id + " " + item.product_name + " " + item.item_count);


                        item_New.product_id = db.Products
                            .Where(x => x.product_name == comboBox_nameProduct.SelectedItem.ToString())
                            .Select(x => x.product_id)
                            .FirstOrDefault();

                        item_New.product_name = comboBox_nameProduct.SelectedItem.ToString();
                        item_New.item_count = TextBox_count.Text.Trim();

                        // remove the old information
                        Item item_Old = dataGrid_EditOrder.SelectedItem as Item;
                        col.Remove(item_Old);

                        //add a new customer
                        col.Add(item_New);

                        // Обновить dataGrid_EditOrder
                        //foreach (var u in col)
                        //{
                        //    col.Add(new Item() { product_id = u.product_id, product_name = u.product_name, item_count = u.item_count.ToString() });
                        //}
                        dataGrid_EditOrder.ItemsSource = col;
                    }
                    //else { MessageBox.Show("Выделите строку"); }
                }
            }
            catch (Exception e1)
            {
                MessageBox.Show(e1.Message, "SaveProductInOrder_Button_Click", MessageBoxButton.OK, MessageBoxImage.Error);
            }


        }

        private void dataGrid_EditOrder_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            Item item = (Item)e.Row.DataContext;

            if (TextBox_count.Text == item.item_count && comboBox_nameProduct.SelectedItem == item.product_name)
                e.Row.Background = System.Windows.Media.Brushes.Yellow;
        }
    }
}
