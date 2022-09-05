// See https://aka.ms/new-console-template for more information
 
using System.Text;
using System.Text.Json;
using Flurl.Http; 

Console.WriteLine("Azure Storage Price Comparison");
var pageSize = 100;
var skip = 100;

var mainurl =
"https://prices.azure.com/api/retail/prices?$filter=productName%20eq%20%27Blob%20Storage%27%20and%20skuName%20eq%20%27Hot%20LRS%27%20and%20unitOfMeasure%20eq%20%271%20GB/Month%27";

Console.WriteLine("fetching 0-100");

var results = await mainurl.GetJsonAsync<pricesObject>();
 
var items = new List<Item>();
items.AddRange(results.Items);

while (results.Count == pageSize)
{
    var nextPage = mainurl + "&$skip=" + skip;
    Console.WriteLine("fetching " + (skip + 1) + "-?");
    results = await nextPage.GetJsonAsync<pricesObject>();
    items.AddRange(results.Items);
    skip += pageSize;
}

Console.WriteLine(items.Count + " downloaded");

var orderedItems = items.OrderBy(x => x.retailPrice);
 
var pricesAndRegions = orderedItems.Select(x => new { x.retailPrice, x.armRegionName });
var groupedPricing = pricesAndRegions
    .GroupBy(x => x.retailPrice)
    .ToDictionary(g => g.Key, g => g.ToList()); ;
 
var sb = new StringBuilder(); 

var existingRegions = new List<string>();

foreach (var key in groupedPricing.Keys)
{
    var regions = groupedPricing[key]
        .Select(x => x.armRegionName);

    var newRegions = regions.Where(x => existingRegions.All(y => y != x));

    if (newRegions.Any())
    {
        newRegions = newRegions.OrderBy(x => x).ToList();
        existingRegions.AddRange(newRegions!);

        var regionsJson = JsonSerializer.Serialize(newRegions);
        sb.Append(key.ToString("0.000000") + " " + newRegions.Count().ToString("00") + " region" + (newRegions.Count() == 1 ? "  " : "s "));
        sb.AppendLine(regionsJson);
    }
}

Console.WriteLine(sb);

var orderedItemsJson = JsonSerializer.Serialize(orderedItems);
File.WriteAllText("d://temp//AzureStoragePricing-fullData.json", orderedItemsJson); 
File.WriteAllText("d://temp//AzureStorage-CheapestPricingByRegion.json", sb.ToString());



public class pricesObject
{
    public string BillingCurrency { get; set; }
    public string CustomerEntityId { get; set; }
    public string CustomerEntityType { get; set; }
    public List<Item> Items { get; set; }
    public string NextPageLink { get; set; }
    public int Count { get; set; }
}

public class Item
{
    public string currencyCode { get; set; }
    //public int tierMinimumUnits { get; set; }
    public float retailPrice { get; set; }
    public float unitPrice { get; set; }
    public string armRegionName { get; set; }
    public string location { get; set; }
    //public DateTime effectiveStartDate { get; set; }
    public string meterId { get; set; }
    public string meterName { get; set; }
    public string productId { get; set; }
    public string skuId { get; set; }
    public string productName { get; set; }
    public string skuName { get; set; }
    public string serviceName { get; set; }
    public string serviceId { get; set; }
    public string serviceFamily { get; set; }
    public string unitOfMeasure { get; set; }
    public string type { get; set; }
    //public bool isPrimaryMeterRegion { get; set; }
    //public string armSkuName { get; set; }
}
