using System.Web;

namespace VivaRealScraper;

internal class UrlBuilder
{
    private const string FORMAT_PART_0 =
        "https://glue-api.vivareal.com/v2/listings?" +
        "addressCity={0}" +
        "&addressLocationId={1}" +
        "&addressNeighborhood=" +
        "&addressState={2}" +
        "&addressCountry=" +
        "&addressStreet=" +
        "&addressZone=" +
        "&addressPointLat={3}" +
        "&addressPointLon={4}" +
        "&business={5}" +
        "&facets=amenities" +
        "&unitTypes=" +
        "&unitSubTypes=" +
        "&unitTypesV3=" +
        "&usageTypes=";

    private const string FORMAT_PRICE_MIN = "&priceMin={0}";

    private const string FORMAT_PART_1 =
        "&listingType={0}" +
        "&parentId=null" +
        "&categoryPage=RESULT" +
        "&images=webp" +
        "&stamps=" +
        "&includeFields=search(result(listings(listing(displayAddressType%2Camenities%2CusableAreas%2CconstructionStatus%2ClistingType%2Cdescription%2Ctitle%2CunitTypes%2CnonActivationReason%2CpropertyType%2CunitSubTypes%2Cid%2Cportal%2CparkingSpaces%2Caddress%2Csuites%2CpublicationType%2CexternalId%2Cbathrooms%2CusageTypes%2CtotalAreas%2CadvertiserId%2Cbedrooms%2CpricingInfos%2CshowPrice%2Cstatus%2CadvertiserContact%2CvideoTourLink%2CwhatsappNumber%2Cstamps)%2Caccount(id%2Cname%2ClogoUrl%2ClicenseNumber%2CshowAddress%2ClegacyVivarealId%2Cphones%2Ctier)%2Cmedias%2CaccountLink%2Clink))%2CtotalCount)%2Cpage%2CseasonalCampaigns%2CfullUriFragments%2Cnearby(search(result(listings(listing(displayAddressType%2Camenities%2CusableAreas%2CconstructionStatus%2ClistingType%2Cdescription%2Ctitle%2CunitTypes%2CnonActivationReason%2CpropertyType%2CunitSubTypes%2Cid%2Cportal%2CparkingSpaces%2Caddress%2Csuites%2CpublicationType%2CexternalId%2Cbathrooms%2CusageTypes%2CtotalAreas%2CadvertiserId%2Cbedrooms%2CpricingInfos%2CshowPrice%2Cstatus%2CadvertiserContact%2CvideoTourLink%2CwhatsappNumber%2Cstamps)%2Caccount(id%2Cname%2ClogoUrl%2ClicenseNumber%2CshowAddress%2ClegacyVivarealId%2Cphones%2Ctier)%2Cmedias%2CaccountLink%2Clink))%2CtotalCount))%2Cexpansion(search(result(listings(listing(displayAddressType%2Camenities%2CusableAreas%2CconstructionStatus%2ClistingType%2Cdescription%2Ctitle%2CunitTypes%2CnonActivationReason%2CpropertyType%2CunitSubTypes%2Cid%2Cportal%2CparkingSpaces%2Caddress%2Csuites%2CpublicationType%2CexternalId%2Cbathrooms%2CusageTypes%2CtotalAreas%2CadvertiserId%2Cbedrooms%2CpricingInfos%2CshowPrice%2Cstatus%2CadvertiserContact%2CvideoTourLink%2CwhatsappNumber%2Cstamps)%2Caccount(id%2Cname%2ClogoUrl%2ClicenseNumber%2CshowAddress%2ClegacyVivarealId%2Cphones%2Ctier)%2Cmedias%2CaccountLink%2Clink))%2CtotalCount))%2Caccount(id%2Cname%2ClogoUrl%2ClicenseNumber%2CshowAddress%2ClegacyVivarealId%2Cphones%2Ctier%2Cphones)%2Cfacets";

    private const string FORMAT_PART_2 =
        "&size={0}" +
        "&from={1}" +
        "&sort=pricingInfos.price%20ASC%20sortFilter%3ApricingInfos.businessType%3D%27RENTAL%27" +
        "&q=";

    private const string FORMAT_DEVELOPMENT = "&developmentsSize={0}";

    private const string FORMAT_PART_3 =
        "&__vt=control" +
        "&levels=CITY" +
        "&ref=" +
        "&pointRadius=" +
        "&isPOIQuery=";

    private readonly string Part_0;
    private readonly string Part_1;
    private readonly string Part_2;


    private UrlBuilder(string part0, string part1, string part2)
    {
        Part_0 = part0;
        Part_1 = part1;
        Part_2 = part2;
    }

    internal static UrlBuilder GetUrlBuilder(Item input, UrlKind kind)
    {
        string part_2 = FORMAT_PART_2;

        string business;
        string listingType;
        switch (kind)
        {
            case UrlKind.Buy:
                business = "SALE";
                listingType = "USED";
                part_2 += FORMAT_DEVELOPMENT;
                break;
            case UrlKind.Rent:
                business = "RENTAL";
                listingType = "USED";
                part_2 += FORMAT_DEVELOPMENT;
                break;
            case UrlKind.Development:
                business = "SALE";
                listingType = "DEVELOPMENT";
                break;
            default:
                throw new NotImplementedException();
        }

        string city = HttpUtility.UrlEncode(input.City);
        string locationID = HttpUtility.UrlEncode(input.LocationID);
        string state = HttpUtility.UrlEncode(input.State);
        string latitude = HttpUtility.UrlEncode(input.Latitude);
        string longitude = HttpUtility.UrlEncode(input.Logitude);

        string part_0 = string.Format(FORMAT_PART_0, city, locationID, state, latitude, longitude, business);
        string part_1 = string.Format(FORMAT_PART_1, listingType);

        return new UrlBuilder(part_0, part_1, part_2);
    }

    internal Uri GetUrl(int from, int size, int prizeMin)
    {
        string url = Part_0;
        if (prizeMin > 0)
            url += string.Format(FORMAT_PRICE_MIN, prizeMin);
        url += Part_1 + string.Format(Part_2, size, from) + FORMAT_PART_3;
        return new(url);
    }
}

internal enum UrlKind : byte
{
    Buy,
    Rent,
    Development
}