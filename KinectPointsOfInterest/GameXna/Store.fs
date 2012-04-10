namespace KinectPointsOfInterest

open MySql.Data.MySqlClient
open System.Windows.Forms

open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics

open System
    
    module Store=

        type Garment(game, id, name, waist)=
            //inherit DrawableGameComponent(game)

            member this.Name
                with get() = name
            member this.ID
                with get() = (int) id
        
        type Customer()=
            static let mutable height = 0
            static let mutable chest = 0
            static let mutable hips = 0
            static let mutable waist = 0

            static let mutable name = ""
            static let mutable email = ""
            
            static member Height
                with get() = height
                and set(h) = height <- h
            static member Waist
                with get() = waist
                and set(h) = waist <- h
            static member Hips
                with get() = hips
                and set(h) = hips <- h
            static member Chest
                with get() = chest
                and set(h) = chest <- h
            static member Name
                with get() = name
                and set(h) = name <- h
            static member Email
                with get() = email
                and set(h) = email <- h
            static member IsACustomer= //checks if the cusotmer object has had values set.
                not (name.Equals "")

    module Database=

        type DatabaseAccess()=
            
            let connectionStr = "Network Address=localhost;" + "Initial Catalog='kinectfashion';" + "Persist Security Info=no;" + "User Name='root';" + "Password='shAke87n'"
            let connection = new MySql.Data.MySqlClient.MySqlConnection(connectionStr)
            
            member this.getCustomer email password=
                try
                    connection.Open()
                    //get customer details
                    let transactionCustomer = connection.BeginTransaction()
                    let SelectGarmentsInAvailableSizes = "SELECT name, email, height, waist, hips, chest FROM users WHERE email LIKE BINARY '"+ email + "' AND password LIKE BINARY '" + password + "'"
                    let cmd = new MySqlCommand(SelectGarmentsInAvailableSizes, connection,transactionCustomer)
                    cmd.CommandTimeout <- 20
                    let rdr = cmd.ExecuteReader()
                    while (rdr.Read()) do
                        Store.Customer.Name <- string rdr.[0]
                        Store.Customer.Email <- string rdr.[1]
                        Store.Customer.Height <- Int32.Parse ((rdr.[2]).ToString())
                        Store.Customer.Waist <- Int32.Parse ((rdr.[3]).ToString())
                        Store.Customer.Hips <- Int32.Parse ((rdr.[4]).ToString())
                        Store.Customer.Chest <-Int32.Parse ((rdr.[5]).ToString())
                        Diagnostics.Debug.WriteLine(rdr.[0].ToString() + rdr.[1].ToString() + rdr.[2].ToString() + rdr.[3].ToString())
                    rdr.Close()
                    transactionCustomer.Dispose()
                    Store.Customer.IsACustomer
                with 
                    | :? MySqlException as ex -> (MessageBox.Show("Error connecting to database.\n\r" + ex.Message ) |> ignore
                                                  false)
                    | ex-> (MessageBox.Show(ex.Message ) |> ignore
                            false)

            member this.getGarments customer whereClause=
                try
                    if connection.State = Data.ConnectionState.Closed then
                        connection.Open()
                    //get garments
                    let mutable garments:List<Store.Garment> = List.Empty
                    let transactionGarments = connection.BeginTransaction()
                    let SelectGarmentsInAvailableSizes = "SELECT * FROM garments NATURAL JOIN stock AS inStock JOIN size_charts ON size_chart = size_charts.id AND inStock.size = size_charts.size " + whereClause
                    let cmd = new MySqlCommand(SelectGarmentsInAvailableSizes, connection,transactionGarments)
                    cmd.CommandTimeout <- 20
                    let rdr = cmd.ExecuteReader()
                    while (rdr.Read()) do
                        garments <- garments @ [new Store.Garment("game", rdr.[0].ToString(), rdr.[1].ToString(), 100)]
                        Diagnostics.Debug.WriteLine(rdr.[0].ToString() + rdr.[1].ToString())
                    rdr.Close()
                    transactionGarments.Dispose()
                    connection.Close()
                    garments

                with 
                    | :? MySqlException as ex -> (MessageBox.Show("Error connecting to database.\n\r" + ex.Message ) |> ignore
                                                  List<Store.Garment>.Empty )
                    | ex-> (MessageBox.Show(ex.Message ) |> ignore
                            List<Store.Garment>.Empty )

        let dbA = new DatabaseAccess()