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
using System.Windows.Shapes;

namespace WpfApp
{
    /// <summary>
    /// Interaction logic for CreateOrder.xaml
    /// </summary>
    /// 

    public class Item
    {
        public int product_id { get; set; }
        public string product_name { get; set; }
        public string item_count { get; set; }
    }

    public partial class CreateOrder : Window
    {
        public CreateOrder()
        {
            InitializeComponent();
            // Загрузка списка товаров из базы в comboBox_nameProduct
            try
            {
                comboBox_nameProduct.Items.Add("Не выбрано");
                using (OrdersdbEntities db = new OrdersdbEntities())
                {
                    var product = db.Products;
                    foreach (Product u in product)
                        comboBox_nameProduct.Items.Add(u.product_name);
                }
                comboBox_nameProduct.SelectedIndex = 0;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Exception", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddProduct_Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Определение product_id по comboBox_nameProduct
                int product_id_temp = 0;
                using (OrdersdbEntities db = new OrdersdbEntities())
                {
                    product_id_temp = db.Products.Where(x => x.product_name == comboBox_nameProduct.SelectedValue.ToString()).Select(x => x.product_id).First(); 
                }

                if (comboBox_nameProduct.SelectedValue.ToString() != "Не выбрано" && Convert.ToInt32(comboBox_count.Text) > 0)
                {
                    dataGrid_List_of_order_items.Items.Add(new Item() { product_id = product_id_temp, product_name = comboBox_nameProduct.SelectedValue.ToString(), item_count = comboBox_count.Text });
                }
                else
                    MessageBox.Show("Выберите товар или введите количество товаров больше 0");
            }
            catch (Exception e1)
            {
                MessageBox.Show(e1.Message, "Exception", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteProduct_Button_Click(object sender, RoutedEventArgs e)
        {
            var index = dataGrid_List_of_order_items.SelectedIndex;

            // Проверка на то, что была выделена ячейка
            if (index > -1)
            {
                if (MessageBox.Show("Вы действительно хотите удалить запись?", "Предупреждение", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    dataGrid_List_of_order_items.Items.RemoveAt(index);
                }
            }
        }

        private void CreateOrder_Button_Click(object sender, RoutedEventArgs e)
        { 
            // id списка позиций клиента
            int list_of_order_itemsMAX = 0;
            
            try
            {
                using (OrdersdbEntities db = new OrdersdbEntities())
                {
                   // Получить максимальное число list_of_order_items
                   list_of_order_itemsMAX = db.Orders.Select(x => x.list_of_order_items).Max();                  
                }
            }
            catch (Exception e1)
            {
                MessageBox.Show(e1.Message, "Exception", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            list_of_order_itemsMAX++;

            // Проверка заполнения полей формы
            if (NameClient_TextBox.Text == "" || dataGrid_List_of_order_items.Items.Count == 0)
            {
                MessageBox.Show("Введите в поле ФИО клиента значение или у Вас 0 позиций заказа", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else
            {
                try
                {
                    // Выборка данных из dataGrid_List_of_order_items и запись в таблицу List_of_order_items
                    foreach (var item in dataGrid_List_of_order_items.Items.OfType<Item>())
                    {
                        List_of_order_items list_of_order_items = new List_of_order_items();
                        list_of_order_items.list_of_order_items__id = list_of_order_itemsMAX;
                        list_of_order_items.item_product_id = item.product_id;
                        list_of_order_items.item_count = Convert.ToInt32(item.item_count);

                        using (OrdersdbEntities db = new OrdersdbEntities())
                        {
                            db.List_of_order_items.Add(list_of_order_items);
                            db.SaveChanges();
                        }
                    }

                    // Подготовка и запись в таблицу Order
                    Order order = new Order();
                    order.date = DateTime.Now;
                    order.name_client = NameClient_TextBox.Text;
                    order.list_of_order_items = list_of_order_itemsMAX;

                    using (OrdersdbEntities db = new OrdersdbEntities())
                    {
                        db.Orders.Add(order);
                        db.SaveChanges();
                    }

                    MessageBox.Show("Данные успешно обновлены", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Обновить датагрид
                    //MainWindow wnd = new MainWindow();
                    //wnd.Load_dataGrid_Orders();

                    // закрыть окно
                    //CreateOrder createOrder = new CreateOrder();
                    //createOrder.Close();

                    this.Owner.Close();
                }
                catch (Exception e1)
                {
                    MessageBox.Show(e1.Message, "Exception", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
