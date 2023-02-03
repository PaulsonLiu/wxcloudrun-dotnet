#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using aspnetapp;
using Newtonsoft.Json;

public class CounterRequest {
    public string action { get; set; }
}
public class CounterResponse {
    public int data { get; set; }
    public string info { get; set; }
}

namespace aspnetapp.Controllers
{
    [Route("api/count")]
    [ApiController]
    public class CounterController : ControllerBase
    {
        private readonly CounterContext _context;

        private readonly ILogger<CounterController> _logger;

        public CounterController(CounterContext context, ILogger<CounterController> logger)
        {
            _context = context;
            _logger = logger;
        }
        private async Task<Counter> getCounterWithInit()
        {
            var counters = await _context.Counters.ToListAsync();
            if (counters.Count() > 0)
            {
                return counters[0];
            }
            else
            {
                var counter = new Counter { count = 0, createdAt = DateTime.Now, updatedAt = DateTime.Now };
                _context.Counters.Add(counter);
                await _context.SaveChangesAsync();
                return counter;
            }
        }
        // GET: api/count
        [HttpGet]
        public async Task<ActionResult<CounterResponse>> GetCounter()
        {
            try
            {
                var counter = await getCounterWithInit();

                //var reqHeaders = Request.Headers.Select(x=>x.Key).ToList();

                _logger.LogInformation("getwxadevinfo=>Start request");

                using var httpClient = new HttpClient();
                var request = new HttpRequestMessage(HttpMethod.Get, "http://api.weixin.qq.com/wxa/getwxadevinfo");
                //request.Content = new StringContent("");
                var response = httpClient.Send(request);
                using var reader = new StreamReader(response.Content.ReadAsStream());
                var responseBody = reader.ReadToEnd();

                _logger.LogInformation("getwxadevinfo=>" + responseBody);

                return new CounterResponse { data = counter.count, info = responseBody };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return new CounterResponse { data = 0, info = ex.Message };
            }
        }

        // POST: api/Counter
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<CounterResponse>> PostCounter(CounterRequest data)
        {
            if (data.action == "inc") {
                var counter = await getCounterWithInit();
                counter.count += 1;
                counter.updatedAt = DateTime.Now;
                await _context.SaveChangesAsync();
                return new CounterResponse { data = counter.count };
            }
            else if (data.action == "clear") {
                var counter = await getCounterWithInit();
                counter.count = 0;
                counter.updatedAt = DateTime.Now;
                await _context.SaveChangesAsync();
                return new CounterResponse { data = counter.count };
            }
            else {
                return BadRequest();
            }
        }
    }
}
