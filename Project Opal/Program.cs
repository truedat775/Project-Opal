using System;
using System.Data;
using System.Diagnostics;
using System.Threading;

using MySql.Data.MySqlClient;

namespace Project_Opal
{
    public class Program
    {
        static MySqlConnection conn;

        static void Main(string[] args)
        {
            Console.WriteLine("Loading Project Opal");
            Console.WriteLine("Based on AH Tool and PyDarkstar.");
            Console.WriteLine("(c) 2020 Alex10");
            Console.WriteLine("Loading items...");

            string path = AppDomain.CurrentDomain.BaseDirectory + @"Items.csv"; 
            Console.WriteLine(path);

            string ItemCSV = path;

            string[,] values = LoadCSV(ItemCSV);
            int num_rows = values.GetUpperBound(0) + 1;
            int num_cols = values.GetUpperBound(1) + 1;

            Console.WriteLine("Success!");

            //
            // 
            // Configuration Settings
            //////////////////////////////////////////////////
            string ipaddress = "127.0.0.1";
            string port = "3306";
            string database = "tpzdb";
            string name = "root";
            string password = "1";
            string sellername = "Zissou";
            string buyername = "Catra";
            string selldate = "1825482385";
            bool sellitems = true;
            bool listprice = true;
            bool buyitems = true;
            int listquantity = 5;
            int listextracraft = 5;
            int smallstackmult = 10;
            int largestackmult = 180;
            bool largestackvendorprice = true;
            int reqlistprice = 0;
            int itemlvlcap = 99;
            int interval = 3600000;
            bool cleanauctionhouse = false;
            bool cleandeliverybox = true;
            bool pricedebug = true;
            bool listdebug = true;
            bool selldebug = true;
            bool buydebug = true;
            //////////////////////////////////////////////////

            conn = new MySqlConnection("Address = '" + ipaddress.ToString() + "';Port = '" + port.ToString() + "';database='" + database.ToString() + "';Persist Security Info=true;User Name='" + name.ToString() + "';password='" + password.ToString() + "'");
            Console.WriteLine("Address: " + ipaddress);
            Console.WriteLine("Port:" + port);
            Console.WriteLine("Database: " + database);
            Console.WriteLine("Username: " + name);
            Console.WriteLine("Password: ????");
            
            bool loop = true;

            string itemlvl = "";

            string itemlvlstr = "Loading items up to level " + itemlvlcap + ".";
            Console.WriteLine(itemlvlstr);

            Console.WriteLine("Transactions will occur every " + (interval / 60000) + " minutes.");

            itemlvl = "(`level` IS NULL OR (`level` <= " + itemlvlcap.ToString() + " AND ilevel <= " + itemlvlcap.ToString() + ") OR (ilevel <= " + itemlvlcap.ToString() + " AND ilevel <> 0)) AND ";

            while (loop == true)
            {
                try
                {
                    conn.Open();
                    if (conn.State == ConnectionState.Open)
                        Console.WriteLine("Connected to database!");
                    MySqlConnection conn2 = conn.Clone();
                    MySqlConnection conn3 = conn.Clone();
                    MySqlConnection conn4 = conn.Clone();
                    MySqlConnection conn5 = conn.Clone();
                    conn2.Open();
                    conn3.Open();
                    conn4.Open();
                    conn5.Open();

                    string itemquery = "SELECT itemid, name, stacksize, ah, basesell, level, ilevel FROM item_basic NATURAL LEFT JOIN item_equipment WHERE " + itemlvl + "ah <> 99;";
                    string cleanquery = "SELECT * FROM auction_house WHERE seller=0;";
                    string clean2query = "SELECT * FROM delivery_box WHERE charid=0;";

                    MySqlCommand itemCmd = new MySqlCommand(itemquery, conn);
                    MySqlCommand cleanCmd = new MySqlCommand(cleanquery, conn4);
                    MySqlCommand clean2Cmd = new MySqlCommand(clean2query, conn5);
                    Console.WriteLine("Initializing reader...");
                    MySqlDataReader itemReader = itemCmd.ExecuteReader();
                    MySqlDataReader cleanReader = cleanCmd.ExecuteReader();
                    MySqlDataReader clean2Reader = clean2Cmd.ExecuteReader();
                    int counterclean = 0;
                    int counter2clean = 0;
                    int countersingle = 0;
                    int counterstacks12 = 0;
                    int counterstacks99 = 0;
                    int counter2single = 0;
                    int counter2stacks12 = 0;
                    int counter2stacks99 = 0;
                    short isstack = 0, isstack2 = 0;
                    short ahallowed = 0;
                    long itemprice = 0;
                    long stackprice = 0;
                    long AhHasItems = 0;
                    long AhHasStacks = 0;
                    long AhHasPlayerItems = 0;
                    long AhHasPlayerStacks = 0;
                    long loopcount = 0;
                    long loop2count = 0;
                    long loop3count = 0;
                    long loop4count = 0;
                    int listextra = 0;

                    try
                    {
                        if (cleanauctionhouse == true)
                        {
                            Console.WriteLine("Cleaning auction house...");

                            string cleanAuctionHouse = "DELETE FROM auction_house WHERE seller=0;";
                            MySqlCommand cleanUpAuctionHouse = new MySqlCommand(cleanAuctionHouse, conn3);
                            while (cleanReader.Read())
                            {
                                cleanUpAuctionHouse.ExecuteNonQuery();

                                counterclean++;
                            }
                            cleanUpAuctionHouse.Dispose();
                            cleanReader.Close();
                        }
                        if (cleandeliverybox == true)
                        {
                            Console.WriteLine("Cleaning delivery box...");

                            string cleanDeliveryBox = "DELETE FROM delivery_box WHERE charid=0;";
                            MySqlCommand cleanUpDeliveryBox = new MySqlCommand(cleanDeliveryBox, conn3);
                            while (clean2Reader.Read())
                            {
                                cleanUpDeliveryBox.ExecuteNonQuery();

                                counter2clean++;
                            }
                            cleanUpDeliveryBox.Dispose();
                            clean2Reader.Close();
                        }
                    }
                    catch (Exception excp)
                    {
                        Console.WriteLine("Error connecting to " +
                        "the mysql server. Internal error message: " + excp.Message, excp);
                        Console.WriteLine("Error cleaning database!");
                    }
                    Console.WriteLine("Done.");

                    Process[] pname = Process.GetProcessesByName("topaz_search");
                    if (pname.Length == 0)
                    {
                        Console.WriteLine("Project Topaz is not running!");
                        Console.WriteLine("Shutting down.");

                        Thread.Sleep(5000);
                        Environment.Exit(0);
                    }
                    else
                        Console.WriteLine("Project Topaz is currently running.  Continuing proccess...");

                    if (buyitems == true || sellitems == true)
                    {
                        Console.WriteLine("Beginning transactions...");
                        Console.WriteLine("Transactions may take several minutes to complete.");
                    }

                    while (itemReader.Read())
                    {
                        if (conn3.State == ConnectionState.Closed) conn3.Open();
                        isstack = Convert.ToInt16(itemReader[2].ToString());
                        ahallowed = Convert.ToInt16(itemReader[3].ToString());

                        if (isstack > 1) isstack2 = 1;
                        else isstack2 = 0;
                        string checkItemCount = "SELECT COUNT(itemid) AS itemcount FROM auction_house WHERE itemid=" + itemReader[0].ToString() + " AND sale=0 AND stack=0 AND seller=0;";
                        string checkStackCount = "SELECT COUNT(itemid) AS itemcount FROM auction_house WHERE itemid=" + itemReader[0].ToString() + " AND sale=0 AND stack=1  AND seller=0;";
                        string checkItemCount2 = "SELECT COUNT(itemid) AS itemcount FROM auction_house WHERE itemid=" + itemReader[0].ToString() + " AND sale=0 AND stack=0 AND seller>0;";
                        string checkStackCount2 = "SELECT COUNT(itemid) AS itemcount FROM auction_house WHERE itemid=" + itemReader[0].ToString() + " AND sale=0 AND stack=1 AND seller>0;";
                        try
                        {
                            MySqlCommand ItemCountis = new MySqlCommand(checkItemCount, conn3);
                            MySqlDataReader ItemCountisReader = ItemCountis.ExecuteReader();
                            while (ItemCountisReader.Read()) AhHasItems = Convert.ToInt32(ItemCountisReader[0].ToString());
                            if (Convert.ToInt32(itemReader[0].ToString()) == 4103)
                                ItemCountisReader.Dispose();
                            ItemCountisReader.Close();
                            ItemCountis.Dispose();

                            MySqlCommand StackCountis = new MySqlCommand(checkStackCount, conn3);
                            MySqlDataReader StackCountisReader = StackCountis.ExecuteReader();
                            while (StackCountisReader.Read()) AhHasStacks = Convert.ToInt32(StackCountisReader[0].ToString());
                            if (Convert.ToInt32(itemReader[0].ToString()) == 4103)
                                StackCountisReader.Dispose();
                            StackCountisReader.Close();
                            StackCountis.Dispose();

                            MySqlCommand ItemCount2is = new MySqlCommand(checkItemCount2, conn3);
                            MySqlDataReader ItemCount2isReader = ItemCount2is.ExecuteReader();
                            while (ItemCount2isReader.Read()) AhHasPlayerItems = Convert.ToInt32(ItemCount2isReader[0].ToString());
                            if (Convert.ToInt32(itemReader[0].ToString()) == 4103)
                                ItemCountisReader.Dispose();
                            ItemCount2isReader.Close();
                            ItemCount2is.Dispose();

                            MySqlCommand StackCount2is = new MySqlCommand(checkStackCount2, conn3);
                            MySqlDataReader StackCount2isReader = StackCount2is.ExecuteReader();
                            while (StackCount2isReader.Read()) AhHasPlayerStacks = Convert.ToInt32(StackCount2isReader[0].ToString());
                            if (Convert.ToInt32(itemReader[0].ToString()) == 4103)
                                StackCount2isReader.Dispose();
                            StackCount2isReader.Close();
                            StackCount2is.Dispose();
                        }
                        catch (Exception excp)
                        {
                            Console.WriteLine("Error connecting to " +
                            "the mysql server. Internal error message: " + excp.Message, excp);
                            Console.WriteLine("Error Counting Items!");
                        }

                        loopcount = listquantity - AhHasItems;
                        loop2count = listquantity - AhHasStacks;
                        loop3count = AhHasPlayerItems;
                        loop4count = AhHasPlayerStacks;
                        if (ahallowed == 35 || ahallowed == 38 || ahallowed == 39 || ahallowed == 40 || ahallowed == 41 || ahallowed == 42 || ahallowed == 43 || ahallowed == 44 || ahallowed == 46 || ahallowed == 50 || ahallowed == 51 || ahallowed == 59)
                        {
                            listextra = listextracraft;
                            loopcount = (listquantity + listextra) - AhHasItems;
                            loop2count = (listquantity + listextra) - AhHasStacks;
                        }
                        else
                            listextra = 0;

                        if (sellitems == true || buyitems == true)
                        {
                            for (int r = 0; r < num_rows; r++)
                            {
                                string curvalue = values[r, 0].Trim();
                                try
                                {
                                    if (curvalue.ToString() == itemReader[0].ToString())
                                    {
                                        string curprice = values[r, 4].Trim();
                                        itemprice = Convert.ToInt32(curprice.ToString());

                                        if (pricedebug)
                                            Console.WriteLine("Current value for item " + itemReader[0].ToString() + " is " + itemprice.ToString() + " gil.");
                                    }
                                }
                                catch
                                {
                                    Console.WriteLine("Error. Unable to look up price!");

                                    throw;
                                }
                            }
                            if (isstack == 12)
                            {
                                stackprice = (itemprice * smallstackmult);
                            }
                            if (isstack == 99)
                            {
                                if (largestackvendorprice)
                                    stackprice = (Convert.ToInt32(itemReader[4].ToString()) * largestackmult);
                                else
                                    stackprice = (itemprice * largestackmult);
                            }
                        }
                        if (ahallowed != 0 && itemprice > reqlistprice)
                        {
                            if (itemReader[1].ToString().StartsWith("judges"))
                            {
                                string addItemtoAH = "DELETE FROM auction_house WHERE itemid=" + itemReader[0].ToString() + ";";
                                try
                                {
                                    MySqlCommand addItemCmd = new MySqlCommand(addItemtoAH, conn2);
                                    addItemCmd.ExecuteNonQuery();
                                    addItemCmd.Dispose();
                                }
                                catch (Exception excp)
                                {
                                    Console.WriteLine("Error connecting to " +
                                    "the mysql server. Internal error message: " + excp.Message, excp);
                                    Console.WriteLine("Error Deleting Judge's Item!");
                                }
                            }
                            else
                            {
                                if (isstack == 1)
                                {
                                    try
                                    {
                                        if (sellitems == true)
                                        {
                                            if (listprice == true && loopcount == (listquantity + listextra))
                                            {
                                                string listItemtoAH = "INSERT INTO auction_house (itemid,stack,seller,seller_name,`date`,price,buyer_name,sale,sell_date) VALUES (" + itemReader[0].ToString() + ",0,0,'" + sellername.ToString() + "','" + selldate + "'," + itemprice.ToString() + ",'" + buyername.ToString() + "'," + itemprice.ToString() + "," + selldate + ");";
                                                MySqlCommand listItemCmd = new MySqlCommand(listItemtoAH, conn2);
                                                listItemCmd.ExecuteNonQuery();

                                                listItemCmd.Dispose();

                                                if (listdebug)
                                                    Console.WriteLine("Listing price for item " + itemReader[0].ToString() + " is " + itemprice.ToString() + " gil.");
                                            }

                                            string addItemtoAH = "INSERT INTO auction_house (itemid,stack,seller,seller_name,`date`,price) VALUES (" + itemReader[0].ToString() + ",0,0,'" + sellername.ToString() + "','" + selldate + "'," + itemprice.ToString() + ");";
                                            MySqlCommand addItemCmd = new MySqlCommand(addItemtoAH, conn2);
                                            for (int i = 0; i < loopcount; i++)
                                            {
                                                addItemCmd.ExecuteNonQuery();
                                                countersingle++;

                                                if(selldebug == true)
                                                    Console.WriteLine("Selling item " + itemReader[0].ToString() + " for " + itemprice.ToString() + " gil.");
                                            }
                                            addItemCmd.Dispose();
                                        }

                                        if (buyitems == true)
                                        {
                                            string buyItemfromAH = "UPDATE auction_house SET buyer_name='" + buyername.ToString() + "', sale=" + itemprice.ToString() + ", sell_date=1325482386 WHERE itemid=" + itemReader[0].ToString() + " AND stack=0 AND seller>0 AND price<=" + itemprice.ToString() + " AND sale=0;";
                                            MySqlCommand buyItemCmd = new MySqlCommand(buyItemfromAH, conn2);
                                            for (int i = 0; i < loop3count; i++)
                                            {
                                                buyItemCmd.ExecuteNonQuery();
                                                counter2single++;

                                                if (buydebug)
                                                    Console.WriteLine("Bidding for item " + itemReader[0].ToString() + " at " + itemprice.ToString() + " gil.");
                                            }
                                            buyItemCmd.Dispose();
                                        }
                                    }
                                    catch (Exception excp)
                                    {
                                        Console.WriteLine("Error connecting to " +
                                        "the mysql server. Internal error message: " + excp.Message, excp);
                                        Console.WriteLine("Error buying or selling Single Item!");
                                        if (buyitems == true)
                                            Console.WriteLine("Make sure delivery box is emptied.");
                                    }
                                }
                                else
                                {
                                    try
                                    {
                                        if (sellitems == true)
                                        {
                                            if (listprice == true && loopcount == (listquantity + listextra))
                                            {
                                                string listItemtoAH = "INSERT INTO auction_house (itemid,stack,seller,seller_name,`date`,price,buyer_name,sale,sell_date) VALUES (" + itemReader[0].ToString() + ",0,0,'" + sellername.ToString() + "','" + selldate + "'," + itemprice.ToString() + ",'" + buyername.ToString() + "'," + itemprice.ToString() + "," + selldate + ");";
                                                MySqlCommand listItemCmd = new MySqlCommand(listItemtoAH, conn2);
                                                listItemCmd.ExecuteNonQuery();

                                                listItemCmd.Dispose();

                                                if (listdebug)
                                                    Console.WriteLine("Listing price for single item " + itemReader[0].ToString() + " is " + itemprice.ToString() + " gil.");
                                            }

                                            string addItemtoAH = "INSERT INTO auction_house (itemid,stack,seller,seller_name,`date`,price) VALUES (" + itemReader[0].ToString() + ",0,0,'" + sellername.ToString() + "','" + selldate + "'," + itemprice.ToString() + ");";
                                            MySqlCommand addItemCmd = new MySqlCommand(addItemtoAH, conn2);
                                            for (int i = 0; i < loopcount; i++)
                                            {
                                                addItemCmd.ExecuteNonQuery();
                                                countersingle++;

                                                if (selldebug == true)
                                                    Console.WriteLine("Selling single stack item " + itemReader[0].ToString() + " for " + itemprice.ToString() + " gil.");
                                            }
                                            addItemCmd.Dispose();
                                        }

                                        if (buyitems == true)
                                        {
                                            string buyItemfromAH = "UPDATE auction_house SET buyer_name='" + buyername.ToString() + "', sale=" + itemprice.ToString() + ", sell_date=1325482386 WHERE itemid=" + itemReader[0].ToString() + " AND stack=0 AND seller>0 AND price<=" + itemprice.ToString() + " AND sale=0;";
                                            MySqlCommand buyItemCmd = new MySqlCommand(buyItemfromAH, conn2);
                                            for (int i = 0; i < loop3count; i++)
                                            {
                                                buyItemCmd.ExecuteNonQuery();
                                                counter2single++;

                                                if (buydebug)
                                                    Console.WriteLine("Bidding for item " + itemReader[0].ToString() + " at " + itemprice.ToString() + " gil.");
                                            }
                                            buyItemCmd.Dispose();
                                        }
                                    }
                                    catch (Exception excp)
                                    {
                                        Console.WriteLine("Error connecting to " +
                                        "the mysql server. Internal error message: " + excp.Message, excp);
                                        Console.WriteLine("Error buying or selling single Stack Item!");
                                        if (buyitems == true)
                                            Console.WriteLine("Make sure delivery box is emptied.");
                                    }
                                    try
                                    {
                                        if (sellitems == true)
                                        {
                                            if (listprice == true && loop2count == (listquantity + listextra))
                                            {
                                                string listItemStacktoAH = "INSERT INTO auction_house (itemid,stack,seller,seller_name,`date`,price,buyer_name,sale,sell_date) VALUES (" + itemReader[0].ToString() + ",1,0,'" + sellername.ToString() + "','" + selldate + "'," + stackprice.ToString() + ",'" + buyername.ToString() + "'," + stackprice.ToString() + "," + selldate + ");";
                                                MySqlCommand listItemStackCmd = new MySqlCommand(listItemStacktoAH, conn2);
                                                listItemStackCmd.ExecuteNonQuery();

                                                listItemStackCmd.Dispose();

                                                if (listdebug)
                                                    Console.WriteLine("Listing price for stack of " + isstack.ToString() + "s item " + itemReader[0].ToString() + " is " + stackprice.ToString() + " gil.");
                                            }

                                            string addItemStacktoAH = "INSERT INTO auction_house (itemid,stack,seller,seller_name,`date`,price) VALUES (" + itemReader[0].ToString() + ",1,0,'" + sellername.ToString() + "','" + selldate + "'," + stackprice.ToString() + ");";
                                            MySqlCommand addItem2Cmd = new MySqlCommand(addItemStacktoAH, conn2);
                                            for (int i = 0; i < loop2count; i++)
                                            {
                                                addItem2Cmd.ExecuteNonQuery();
                                                if (isstack == 12)
                                                {
                                                    counterstacks12++;
                                                }
                                                if (isstack == 99)
                                                {
                                                    counterstacks99++;
                                                }

                                                if (selldebug == true)
                                                    Console.WriteLine("Selling stack of " + isstack.ToString() + " item " + itemReader[0].ToString() + "s for " + stackprice.ToString() + " gil.");
                                            }
                                            addItem2Cmd.Dispose();
                                        }

                                        if (buyitems == true)
                                        {
                                            string buyItemfromAH = "UPDATE auction_house SET buyer_name='" + buyername.ToString() + "', sale=" + stackprice.ToString() + ", sell_date=1325482386 WHERE itemid=" + itemReader[0].ToString() + " AND stack=1 AND seller>0 AND price<=" + stackprice.ToString() + " AND sale=0;";
                                            MySqlCommand buyItemStackCmd = new MySqlCommand(buyItemfromAH, conn2);
                                            for (int i = 0; i < loop4count; i++)
                                            {
                                                buyItemStackCmd.ExecuteNonQuery();
                                                if (isstack == 12)
                                                {
                                                    counter2stacks12++;
                                                }
                                                if (isstack == 99)
                                                {
                                                    counter2stacks99++;
                                                }

                                                if (buydebug)
                                                    Console.WriteLine("Bidding for stack item " + itemReader[0].ToString() + " at " + itemprice.ToString() + " gil.");
                                            }
                                            buyItemStackCmd.Dispose();
                                        }
                                    }
                                    catch (Exception excp)
                                    {
                                        Console.WriteLine("Error connecting to " +
                                        "the mysql server. Internal error message: " + excp.Message, excp);
                                        Console.WriteLine("Error buying or selling Stack Item!");
                                        if(buyitems == true)
                                            Console.WriteLine("Make sure delivery box is emptied.");
                                    }
                                }
                            }
                        }
                        itemprice = 0;
                    }
                    itemReader.Close();
                    if (conn.State == ConnectionState.Open) conn.Close();
                    if (conn2.State == ConnectionState.Open) conn2.Close();
                    if (conn3.State == ConnectionState.Open) conn3.Close();
                    if (conn4.State == ConnectionState.Open) conn4.Close();
                    if (conn5.State == ConnectionState.Open) conn5.Close();
                    int totalitems = countersingle + counterstacks12 + counterstacks99;
                    int totalitems2 = counter2single + counter2stacks12 + counter2stacks99;
                    Console.WriteLine("Auction house updated...");
                    Console.WriteLine("All Judge's items deleted from AH.");
                    if (cleanauctionhouse == true)
                    {
                        Console.WriteLine("Items cleaned from AH: " + counterclean.ToString());
                    }
                    if (cleandeliverybox == true)
                    {
                        Console.WriteLine("Slots cleaned in DB: " + counter2clean.ToString());
                    }
                    if (sellitems == true)
                    {
                        Console.WriteLine("Single items added: " + countersingle.ToString());
                        Console.WriteLine("Stack of 12 items added: " + counterstacks12.ToString());
                        Console.WriteLine("Stack of 99 items added: " + counterstacks99.ToString());
                        Console.WriteLine("Total items added to AH: " + totalitems.ToString());
                    }
                    if (buyitems == true)
                    {
                        Console.WriteLine("Single items bought: " + counter2single.ToString());
                        Console.WriteLine("Stack of 12 items bought: " + counter2stacks12.ToString());
                        Console.WriteLine("Stack of 99 items bought: " + counter2stacks99.ToString());
                        Console.WriteLine("Total items bought from AH: " + totalitems2.ToString());
                        if(buydebug == true)
                            Console.WriteLine("Notice: If you didn't receive any gil from the AH you may need to execute triggers.sql script.");
                    }
                    Console.WriteLine(System.DateTime.Now.ToString());
                }
                catch (Exception excp)
                {
                    Console.WriteLine("Error connecting to " +
                        "the mysql server. Internal error message: " + excp.Message, excp);
                    Console.WriteLine("Error Connecting to MySQL Server!");
                    Console.WriteLine("Please Check your connection Settings.");
                }
                Thread.Sleep(interval);
            }
        }

        private static string[,] LoadCSV(string filename)
        {
            string whole_file = System.IO.File.ReadAllText(filename);

            whole_file = whole_file.Replace('\n', '\r');
            string[] lines = whole_file.Split(new char[] { '\r' },
                StringSplitOptions.RemoveEmptyEntries);

            int num_rows = lines.Length;
            int num_cols = lines[0].Split(',').Length;

            string[,] values = new string[num_rows, num_cols];

            for (int r = 0; r < num_rows; r++)
            {
                string[] line_r = lines[r].Split(',');
                for (int c = 0; c < num_cols; c++)
                {
                    values[r, c] = line_r[c];
                }
            }

            return values;
        }
    }
}
