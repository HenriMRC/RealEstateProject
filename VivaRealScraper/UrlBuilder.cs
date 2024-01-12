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

    public UrlBuilder(Item input, UrlKind kind)
    {
        Part_2 = FORMAT_PART_2;

        UrlKindUtility.GetBusinessAndTypeFromUrlKind(kind, out string business, out string listingType);
        switch (kind)
        {
            case UrlKind.Buy:
            case UrlKind.Rent:
                Part_2 += FORMAT_DEVELOPMENT;
                break;
            case UrlKind.Development:
                break;
            default:
                throw new NotImplementedException();
        }

        string city = Uri.EscapeDataString(input.City);
        string locationID = Uri.EscapeDataString(input.LocationID);
        string state = Uri.EscapeDataString(input.State);
        string latitude = Uri.EscapeDataString(input.Latitude);
        string longitude = Uri.EscapeDataString(input.Longitude);

        Part_0 = string.Format(FORMAT_PART_0, city, locationID, state, latitude, longitude, business);
        Part_1 = string.Format(FORMAT_PART_1, listingType);
    }

    internal string GetUrl(int from, int size, int priceMin)
    {
        string url = Part_0;
        if (priceMin > 0)
            url += string.Format(FORMAT_PRICE_MIN, priceMin);
        url += Part_1 + string.Format(Part_2, size, from) + FORMAT_PART_3;
        return url;
    }
}

public enum UrlKind : byte
{
    Buy,
    Rent,
    Development
}

public static class UrlKindUtility
{
    public static void GetBusinessAndTypeFromUrlKind(UrlKind kind, out string business, out string listingType)
    {
        switch (kind)
        {
            case UrlKind.Buy:
                business = "SALE";
                listingType = "USED";
                break;
            case UrlKind.Rent:
                business = "RENTAL";
                listingType = "USED";
                break;
            case UrlKind.Development:
                business = "SALE";
                listingType = "DEVELOPMENT";
                break;
            default:
                throw new NotImplementedException();
        }

    }
}