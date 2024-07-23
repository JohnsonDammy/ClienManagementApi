using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SqlClient;
using ClientManagementAPI.Models;

namespace ClientManagementAPI.Repositories
{
    public class ClientRepository
    {
        private string connectionString = @"server=DAMALIRE-LT\SQLEXPRESS07;database=ClientManagementDB;integrated security=true";

        public List<Client> GetAllClients()
        {
            var clients = new List<Client>();
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT * FROM Clients";
                SqlCommand cmd = new SqlCommand(query, conn);
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    clients.Add(new Client
                    {
                        Id = (int)reader["Id"],
                        Name = reader["Name"].ToString(),
                        Gender = reader["Gender"].ToString(),
                        Details = reader["Details"].ToString()
                    });
                }
            }
            return clients;
        }

        public bool AddClient(Client client)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "INSERT INTO Clients (Name, Gender, Details) VALUES (@Name, @Gender, @Details)";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Name", client.Name);
                cmd.Parameters.AddWithValue("@Gender", client.Gender);
                cmd.Parameters.AddWithValue("@Details", client.Details);
                conn.Open();
                int rowsAffected = cmd.ExecuteNonQuery();
                return rowsAffected > 0;
            }
        }

        // Methods for addresses
        public List<Address> GetAddressesByClientId(int clientId)
        {
            var addresses = new List<Address>();
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT * FROM Addresses WHERE ClientId = @ClientId";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@ClientId", clientId);
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    addresses.Add(new Address
                    {
                        Id = (int)reader["Id"],
                        ClientId = (int)reader["ClientId"],
                        AddressType = reader["AddressType"].ToString(),
                        AddressLine = reader["AddressLine"].ToString()
                    });
                }
            }
            return addresses;
        }

        public bool AddAddress(Address address)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "INSERT INTO Addresses (ClientId, AddressType, AddressLine) VALUES (@ClientId, @AddressType, @AddressLine)";
                SqlCommand cmd = new SqlCommand(query, conn);

                // Debugging lines
                Console.WriteLine($"ClientId: {address.ClientId}");
                Console.WriteLine($"AddressType: {address.AddressType}");
                Console.WriteLine($"AddressLine: {address.AddressLine}");

                cmd.Parameters.AddWithValue("@ClientId", address.ClientId);
                cmd.Parameters.AddWithValue("@AddressType", string.IsNullOrEmpty(address.AddressType) ? (object)DBNull.Value : address.AddressType);
                cmd.Parameters.AddWithValue("@AddressLine", string.IsNullOrEmpty(address.AddressLine) ? (object)DBNull.Value : address.AddressLine);

                conn.Open();
                int rowsAffected = cmd.ExecuteNonQuery();
                return rowsAffected > 0;
            }
        }



        // Methods for contacts
        public List<Contact> GetContactsByClientId(int clientId)
        {
            var contacts = new List<Contact>();
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT * FROM Contacts WHERE ClientId = @ClientId";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@ClientId", clientId);
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    contacts.Add(new Contact
                    {
                        Id = (int)reader["Id"],
                        ClientId = (int)reader["ClientId"],
                        ContactType = reader["ContactType"].ToString(),
                        ContactNumber = reader["ContactNumber"].ToString()
                    });
                }
            }
            return contacts;
        }

        public bool AddContact(Contact contact)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "INSERT INTO Contacts (ClientId, ContactType, ContactNumber) VALUES (@ClientId, @ContactType, @ContactNumber)";
                SqlCommand cmd = new SqlCommand(query, conn);

                cmd.Parameters.AddWithValue("@ClientId", contact.ClientId);
                cmd.Parameters.AddWithValue("@ContactType", contact.ContactType ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@ContactNumber", contact.ContactNumber ?? (object)DBNull.Value);

                conn.Open();
                int rowsAffected = cmd.ExecuteNonQuery();
                return rowsAffected > 0;
            }
        }

        // Export clients and addresses
        public List<ClientWithAddresses> GetClientsWithAddresses()
        {
            var clients = new List<ClientWithAddresses>();
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT c.Id, c.Name, c.Gender, c.Details, a.AddressType, a.AddressLine " +
                               "FROM Clients c " +
                               "LEFT JOIN Addresses a ON c.Id = a.ClientId";
                SqlCommand cmd = new SqlCommand(query, conn);
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    var client = clients.FirstOrDefault(c => c.Id == (int)reader["Id"]);
                    if (client == null)
                    {
                        client = new ClientWithAddresses
                        {
                            Id = (int)reader["Id"],
                            Name = reader["Name"].ToString(),
                            Gender = reader["Gender"].ToString(),
                            Details = reader["Details"].ToString(),
                            Addresses = new List<Address>()
                        };
                        clients.Add(client);
                    }
                    if (reader["AddressLine"] != DBNull.Value)
                    {
                        client.Addresses.Add(new Address
                        {
                            AddressType = reader["AddressType"].ToString(),
                            AddressLine = reader["AddressLine"].ToString()
                        });
                    }
                }
            }
            return clients;
        }


        public List<ClientWithDetails> GetClientsWithDetails()
        {
            var clients = new List<ClientWithDetails>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT c.Id, c.Name, c.Gender, c.Details, a.AddressType, a.AddressLine, " +
                               "ct.ContactType, ct.ContactNumber " +
                               "FROM Clients c " +
                               "LEFT JOIN Addresses a ON c.Id = a.ClientId " +
                               "LEFT JOIN Contacts ct ON c.Id = ct.ClientId";

                SqlCommand cmd = new SqlCommand(query, conn);
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    var client = clients.FirstOrDefault(c => c.Id == (int)reader["Id"]);
                    if (client == null)
                    {
                        client = new ClientWithDetails
                        {
                            Id = (int)reader["Id"],
                            Name = reader["Name"].ToString(),
                            Gender = reader["Gender"].ToString(),
                            Details = reader["Details"].ToString(),
                            Addresses = new List<Address>(),
                            Contacts = new List<Contact>()
                        };
                        clients.Add(client);
                    }

                    if (reader["AddressLine"] != DBNull.Value)
                    {
                        client.Addresses.Add(new Address
                        {
                            AddressType = reader["AddressType"].ToString(),
                            AddressLine = reader["AddressLine"].ToString()
                        });
                    }

                    if (reader["ContactNumber"] != DBNull.Value)
                    {
                        client.Contacts.Add(new Contact
                        {
                            ContactType = reader["ContactType"].ToString(),
                            ContactNumber = reader["ContactNumber"].ToString()
                        });
                    }
                }
            }
            return clients;
        }

    }
}
