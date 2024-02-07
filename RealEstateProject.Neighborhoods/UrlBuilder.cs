using RealEstateProject.Data;

namespace RealEstateProject.Neighborhoods;

internal class UrlBuilder
{
    //"https://glue-api.vivareal.com/v3/locations?portal=VIVAREAL" +
    //"&fields=neighborhood" +
    //"&includeFields=address.neighborhood%2Caddress.city%2Caddress.state%2Caddress.zone%2Caddress.locationId%2Caddress.point%2Curl%2Cadvertiser.name%2CuriCategory.page%2Ccondominium.name%2Caddress.street" +
    //"&size=110" +
    //"&from=110" +
    //"&q=S%C3%A3o+Paulo+-+SP" +
    //"&amenities=Amenity_NONE" +
    //"&constructionStatus=ConstructionStatus_NONE" +
    //"&listingType=USED" +
    //"&businessType=SALE" +
    //"&unitTypes=" +
    //"&usageTypes=" +
    //"&unitSubTypes=" +
    //"&unitTypesV3=" +
    //"&__vt=";

    //"https://glue-api.vivareal.com/v3/locations?portal=VIVAREAL" +
    //"&fields=neighborhood" +
    //"&includeFields=address.neighborhood%2Caddress.city%2Caddress.state%2Caddress.zone%2Caddress.locationId%2Caddress.point%2Curl%2Cadvertiser.name%2CuriCategory.page%2Ccondominium.name%2Caddress.street" +
    //"&size=110" +
    //"&from=110" +
    //"&q=S%C3%A3o+Paulo+-+SP" +
    //"&amenities=Amenity_NONE" +
    //"&constructionStatus=ConstructionStatus_NONE" +
    //"&listingType=USED" +
    //"&businessType=RENTAL" +
    //"&unitTypes=" +
    //"&usageTypes=" +
    //"&unitSubTypes=" +
    //"&unitTypesV3=" +
    //"&__vt=";

    //"https://glue-api.vivareal.com/v3/locations?portal=VIVAREAL" +
    //"&fields=neighborhood" +
    //"&includeFields=address.neighborhood%2Caddress.city%2Caddress.state%2Caddress.zone%2Caddress.locationId%2Caddress.point%2Curl%2Cadvertiser.name%2CuriCategory.page%2Ccondominium.name" +
    //"&size=110" +
    //"&from=110" +
    //"&q=S%C3%A3o+Paulo+-+SP" +
    //"&amenities=Amenity_NONE" +
    //"&constructionStatus=ConstructionStatus_NONE" +
    //"&listingType=DEVELOPMENT" +
    //"&businessType=SALE" +
    //"&unitTypes=" +
    //"&usageTypes=" +
    //"&unitSubTypes=" +
    //"&unitTypesV3=" +
    //"&__vt=";

    private const string PART_0 = "https://glue-api.vivareal.com/v3/locations?portal=VIVAREAL&fields=neighborhood";
    private const string PART_1 = "&includeFields=address.neighborhood%2Caddress.city%2Caddress.state%2Caddress.zone%2Caddress.locationId%2Caddress.point%2Curl%2Cadvertiser.name%2CuriCategory.page%2Ccondominium.name";
    private const string PART_1_OPTION = "%2Caddress.street";
    private const string PART_2 = "&size=";
    private const string PART_3 = "&from=";
    private const string PART_4 = "&q=";
    private const string PART_5 = "&amenities=Amenity_NONE&constructionStatus=ConstructionStatus_NONE";
    private const string PART_6 = "&listingType=";
    private const string PART_7 = "&businessType=";
    private const string PART_8 = "&unitTypes=&usageTypes=&unitSubTypes=&unitTypesV3=&__vt=";

    private readonly string Part_0;
    private readonly string Part_1;

    public UrlBuilder(string input, UrlKind kind)
    {
        input = Uri.EscapeDataString(input);

        Part_0 = PART_0 + PART_1;
        Part_1 = PART_4 + input + PART_5;

        switch (kind)
        {
            case UrlKind.Sale:
                Part_0 += PART_1_OPTION;

                Part_1 += PART_6 + "USED";
                Part_1 += PART_7 + "SALE";
                Part_1 += PART_8;
                break;
            case UrlKind.Rent:
                Part_0 += PART_1_OPTION;

                Part_1 += PART_6 + "USED";
                Part_1 += PART_7 + "RENTAL";
                Part_1 += PART_8;
                break;
            case UrlKind.Development:
                Part_1 += PART_6 + "DEVELOPMENT";
                Part_1 += PART_7 + "SALE";
                Part_1 += PART_8;
                break;
            default:
                throw new NotImplementedException();
        }
    }

    internal string GetUrl(int from, int size)
    {
        return Part_0 + PART_2 + size + (from > 0 ? (PART_3 + from) : string.Empty) + Part_1;
    }
}
