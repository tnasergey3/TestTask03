using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public MainWindow()
        {

            InitializeComponent();

            Load_dataGrid_Orders();

            //// Загрузка данными из базы dataGrid_Orders
            //try
            //{
            //    using (OrdersdbEntities db = new OrdersdbEntities())
            //    {
            //        var dataFromQuary = from o in db.Orders.AsEnumerable()
            //                            from l in db.List_of_order_items
            //                            from p in db.Products
            //                            where o.list_of_order_items == l.list_of_order_items__id
            //                            where l.item_product_id == p.product_id
            //                            group new { db.Orders, db.List_of_order_items, db.Products, l.item_count, p.product_price }
            //                            by new { o.orders_id, o.date, o.name_client } into g
            //                            select new
            //                            {
            //                                orders_id = g.Key.orders_id,
            //                                date = g.Key.date.ToShortDateString(),
            //                                name_client = g.Key.name_client,
            //                                totalSumOrder = g.Sum(x => x.item_count * x.product_price)
            //                            };

            //        dataGrid_Orders.ItemsSource = dataFromQuary;
            //    }
            //}
            //catch (Exception e)
            //{
            //    MessageBox.Show(e.Message, "Exception", MessageBoxButton.OK, MessageBoxImage.Error);
            //}
        }

        // Загрузка данных из базы dataGrid_Orders
        public void Load_dataGrid_Orders()
        {            
            try
            {
                using (OrdersdbEntities db = new OrdersdbEntities())
                {
                    var dataFromQuary = from o in db.Orders.AsEnumerable()
                                        from l in db.List_of_order_items
                                        from p in db.Products
                                        where o.list_of_order_items == l.list_of_order_items__id
                                        where l.item_product_id == p.product_id
                                        group new { db.Orders, db.List_of_order_items, db.Products, l.item_count, p.product_price }
                                        by new { o.orders_id, o.date, o.name_client } into g
                                        select new
                                        {
                                            orders_id = g.Key.orders_id,
                                            date = g.Key.date.ToShortDateString(),
                                            name_client = g.Key.name_client,
                                            totalSumOrder = g.Sum(x => x.item_count * x.product_price)
                                        };

                    dataGrid_Orders.ItemsSource = dataFromQuary;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Exception", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Обработка события "Нажатие мыши на данные dataGrid_Orders"
        private void dataGrid_Orders_MouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                // Проверка на то, что клик мышки будет на колонке с данными
                if (dataGrid_Orders.CurrentCell.Column != null)
                {
                    // При выборе строки получение ID заказа
                    object item = dataGrid_Orders.SelectedItem;
                    string ID_List_of_order_items = (dataGrid_Orders.SelectedCells[0].Column.GetCellContent(item) as TextBlock).Text;
                    //MessageBox.Show(ID_List_of_order_items);
                    //Debug.WriteLine(ID_List_of_order_items);

                    // Загрузка данных в dataGrid_List_of_order_items
                    using (OrdersdbEntities db = new OrdersdbEntities())
                    {
                       var dataFromQuary = from o in db.Orders.AsEnumerable()
                                           from l in db.List_of_order_items
                                           from p in db.Products
                                           where o.orders_id == Convert.ToInt32(ID_List_of_order_items)
                                           where l.list_of_order_items__id == o.list_of_order_items
                                           where l.item_product_id == p.product_id
                                           select new
                                           {
                                               product_id = p.product_id,
                                               product_name = p.product_name,
                                               item_count = l.item_count,
                                               product_price = p.product_price
                                           };

                        dataGrid_List_of_order_items.ItemsSource = dataFromQuary;
                    }
                }                              
            }
            catch (Exception e2)
            {
                MessageBox.Show(e2.Message, "dataGrid_Orders_MouseUp", MessageBoxButton.OK, MessageBoxImage.Error);
            }            
        }

        private void CreateOrder_Button_Click(object sender, RoutedEventArgs e)
        {
            CreateOrder createOrder = new CreateOrder();
            createOrder.ShowDialog();
        }

        private void EditOrder_Button_Click(object sender, RoutedEventArgs e)
        {
            //MessageBox.Show(sender.ToString());

        }

        private void DeleteOrder_Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var index = dataGrid_Orders.SelectedIndex;
                // Проверка на то, что была выделена ячейка
                if (index > -1)
                {
                    if (MessageBox.Show("Вы действительно хотите удалить заказ?", "Удаление заказа", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                    {
                        // При выборе строки получение ID заказа
                        object item = dataGrid_Orders.SelectedItem;
                        string ID_List_of_order_items = (dataGrid_Orders.SelectedCells[0].Column.GetCellContent(item) as TextBlock).Text;
                        Debug.WriteLine(ID_List_of_order_items);

                        //this.dataGrid_Orders.ItemsSource = dataTable.DefaultView;
                        //dataGrid_Orders.Items.RemoveAt(index);

                        dataGrid_List_of_order_items.ItemsSource = null;
                    }
                }                




                
            }
            catch (Exception e2)
            {
                MessageBox.Show(e2.Message, "DeleteOrder_Button_Click", MessageBoxButton.OK, MessageBoxImage.Error);
            } 
        }

        private void UpdateOrder_Button_Click(object sender, RoutedEventArgs e)
        {
            Load_dataGrid_Orders();
        }
    }
}
