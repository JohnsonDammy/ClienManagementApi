using System;
using System.Collections.Generic;
using System.Linq;
using System.Text; // For StringBuilder
using System.Web.Http;
using System.Web.Http.Cors;
using System.Net.Http; // For HttpResponseMessage
using System.Net.Http.Headers; // For MediaTypeHeaderValue
using ClientManagementAPI.Repositories;
using ClientManagementAPI.Models;
using OfficeOpenXml; // For EPPlus
using System.Data; // For DataTable
using System.IO; // For MemoryStream

namespace ClientManagementAPI.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    [RoutePrefix("api/clients")]
    public class ClientController : ApiController
    {
        private ClientRepository repo = new ClientRepository();

        [HttpGet]
        [Route("")]
        public IHttpActionResult GetAllClients()
        {
            var clients = repo.GetClientsWithDetails();
            return Ok(clients);
        }

        [HttpPost]
        [Route("")]
        public IHttpActionResult CreateClient(Client client)
        {
            bool success = repo.AddClient(client);
            if (success)
                return CreatedAtRoute("DefaultApi", new { id = client.Id }, client);

            return BadRequest("Unable to create client");
        }

        [HttpGet]
        [Route("export")]
        public HttpResponseMessage ExportClients()
        {
            var clients = repo.GetClientsWithAddresses(); // Fetch clients with addresses

            var csv = "Id,Name,Gender,Details,AddressType,AddressLine\n";
            foreach (var client in clients)
            {
                foreach (var address in client.Addresses)
                {
                    csv += $"{client.Id},{client.Name},{client.Gender},{client.Details},{address.AddressType},{address.AddressLine}\n";
                }
            }

            var csvBytes = System.Text.Encoding.UTF8.GetBytes(csv);
            var result = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(csvBytes)
            };
            result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = "clients_with_addresses.csv"
            };
            result.Content.Headers.ContentType = new MediaTypeHeaderValue("text/csv");

            return result;
        }



        [HttpGet]
        [Route("{clientId}/addresses")]
        public IHttpActionResult GetAddresses(int clientId)
        {
            var addresses = repo.GetAddressesByClientId(clientId);
            return Ok(addresses);
        }

        [HttpPost]
        [Route("{clientId}/addresses")]
        public IHttpActionResult AddAddress(int clientId, Address address)
        {
            address.ClientId = clientId;
            var success = repo.AddAddress(address);
            return success ? (IHttpActionResult)Ok() : BadRequest();
        }

        [HttpGet]
        [Route("{clientId}/contacts")]
        public IHttpActionResult GetContacts(int clientId)
        {
            var contacts = repo.GetContactsByClientId(clientId);
            return Ok(contacts);
        }

        [HttpPost]
        [Route("{clientId}/contacts")]
        public IHttpActionResult AddContact(int clientId, Contact contact)
        {
            contact.ClientId = clientId;
            var success = repo.AddContact(contact);
            return success ? (IHttpActionResult)Ok() : BadRequest();
        }

        [HttpGet]
        [Route("export-with-addresses")]
        public IHttpActionResult ExportClientsWithAddresses()
        {
            var clients = repo.GetClientsWithAddresses();
            var csv = GenerateCsv(clients);
            var csvBytes = Encoding.UTF8.GetBytes(csv);
            var result = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(csvBytes)
            };
            result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = "clients_with_addresses.csv"
            };
            result.Content.Headers.ContentType = new MediaTypeHeaderValue("text/csv");

            return ResponseMessage(result);
        }

        private string GenerateCsv(List<ClientWithAddresses> clients)
        {
            var csv = new StringBuilder();
            csv.AppendLine("Id,Name,Gender,Details,AddressType,AddressLine");
            foreach (var client in clients)
            {
                foreach (var address in client.Addresses)
                {
                    csv.AppendLine($"{client.Id},{client.Name},{client.Gender},{client.Details},{address.AddressType},{address.AddressLine}");
                }
            }
            return csv.ToString();
        }


        [HttpGet]
        [Route("export-excel")]
        public HttpResponseMessage ExportClientsExcel()
        {
            // Set the license context for EPPlus
            OfficeOpenXml.ExcelPackage.LicenseContext = LicenseContext.NonCommercial; // Use Commercial if applicable

            var clients = repo.GetClientsWithAddresses(); // Fetch clients with addresses

            var dataTable = new System.Data.DataTable();
            dataTable.Columns.Add("Id");
            dataTable.Columns.Add("Name");
            dataTable.Columns.Add("Gender");
            dataTable.Columns.Add("Details");
            dataTable.Columns.Add("AddressType");
            dataTable.Columns.Add("AddressLine");

            foreach (var client in clients)
            {
                foreach (var address in client.Addresses)
                {
                    dataTable.Rows.Add(client.Id, client.Name, client.Gender, client.Details, address.AddressType, address.AddressLine);
                }
            }

            using (var package = new OfficeOpenXml.ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Clients");
                worksheet.Cells["A1"].LoadFromDataTable(dataTable, true);
                var stream = new System.IO.MemoryStream();
                package.SaveAs(stream);

                // Prepare the response
                var result = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
                {
                    Content = new ByteArrayContent(stream.ToArray())
                };
                result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                {
                    FileName = "clients_with_addresses.xlsx"
                };
                result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");

                return result;
            }
        }


    }
}
