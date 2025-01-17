using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using CsvHelper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Minio;
using Minio.DataModel.Args;
using Minio.Exceptions;
using sample_dotnet_webapi.Models;

namespace sample_dotnet_webapi.Controllers
{
    [Route("api/[controller]")]
    public class PersonsController : MyControllerBase
    {
        private readonly DataContext _context;
        private readonly IMinioClient _minioClient;
        private readonly string _s3bucket = "sample-dotnet-webapi-bucket";

        public PersonsController(DataContext context, IMinioClient minioClient)
        {
            _context = context;
            _minioClient = minioClient;
        }

        // GET: api/Persons
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Person>>> GetPersons()
        {
            return await _context.Persons.ToListAsync();
        }

        // GET: api/Persons/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Person>> GetPerson(Guid id)
        {
            var person = await _context.Persons.FindAsync(id);

            if (person == null)
            {
                return NotFound();
            }

            return person;
        }

        // GET: api/Persons/csv
        // Gets the GUID for the object and the presigned link for CSV upload
        [HttpGet("csv")]
        public async Task<ActionResult<csvUploadDetails>> GetPersonsCsv(Guid id)
        {
            var objGuid = Guid.NewGuid().ToString();
            PresignedPutObjectArgs args = new PresignedPutObjectArgs()
                .WithBucket(_s3bucket)
                .WithObject(objGuid)
                .WithExpiry(60 * 10 * 1);
            return new csvUploadDetails
            {
                ObjName = objGuid,
                UploadLink = await _minioClient.PresignedPutObjectAsync(args),
            };
        }

        public struct csvUploadDetails
        {
            public string ObjName { get; set; }
            public string UploadLink { get; set; }
        }

        // PUT: api/Persons/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPerson(Guid id, Person person)
        {
            if (id != person.PersonId)
            {
                return BadRequest();
            }

            _context.Entry(person).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PersonExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Persons
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Person>> PostPerson(Person person)
        {
            _context.Persons.Add(person);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPerson), new { id = person.PersonId }, person);
        }

        // POST: api/Persons/csv
        // Parses and saves the CSV from S3/R2 to DB
        [HttpPost("csv")]
        public async Task<IActionResult> PostPersonsCsv([FromBody] string objectName)
        {
            var emptyFlag = true;

            // Check whether the object exists using statObject().
            // If the object is not found, statObject() throws an exception,
            // else it means that the object exists.
            // Execution is successful.

            try
            {
                StatObjectArgs statObjectArgs = new StatObjectArgs()
                    .WithBucket(_s3bucket)
                    .WithObject(objectName);
                await _minioClient.StatObjectAsync(statObjectArgs);

                // Get input stream
                GetObjectArgs getObjectArgs = new GetObjectArgs()
                    .WithBucket(_s3bucket)
                    .WithObject(objectName)
                    .WithCallbackStream(
                        (stream) =>
                        {
                            using (StreamReader reader = new StreamReader(stream))
                            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                            {
                                csv.Read();
                                csv.ReadHeader();
                                while (csv.Read())
                                {
                                    csv.Context.RegisterClassMap<PersonMap>();

                                    var persons = csv.GetRecords<Person>();

                                    foreach (var person in persons)
                                    {
                                        var isValid = Validator.TryValidateObject(
                                            person,
                                            new ValidationContext(person),
                                            new List<ValidationResult>(),
                                            true
                                        );

                                        if (!isValid)
                                        {
                                            Console.WriteLine(
                                                $"Discarding invalid record: {JsonSerializer.Serialize(person)}"
                                            );
                                            continue;
                                        }

                                        emptyFlag = false;
                                        _context.Persons.Add(person);
                                    }
                                }
                            }
                        }
                    );

                await _minioClient.GetObjectAsync(getObjectArgs);
            }
            catch (ObjectNotFoundException e)
            {
                Console.WriteLine($"Error occurred: {e}");
                return NotFound();
            }

            await _context.SaveChangesAsync();

            if (emptyFlag)
                return BadRequest("No valid records found in the specified file");

            return NoContent();
        }

        // DELETE: api/Persons/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePerson(Guid id)
        {
            var person = await _context.Persons.FindAsync(id);
            if (person == null)
            {
                return NotFound();
            }

            _context.Persons.Remove(person);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool PersonExists(Guid id)
        {
            return _context.Persons.Any(e => e.PersonId == id);
        }
    }
}