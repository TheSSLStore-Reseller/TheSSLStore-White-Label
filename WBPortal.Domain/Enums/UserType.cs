namespace WBSSLStore.Domain
{
    public enum UserType
    {
        CUSTOMER = 0,
        RESELLER = 1,
        ADMIN = 100,//Lets assume more types might be added like Affiliate, hence random number
        SUPPORT = 99,
        FINANCE = 98
    }

    public enum ConfigurationStage
    {
        NoCreated = 0, 
        CreateDB = 1,
        PaymentSetting = 2,
        AdminSetup = 3,
        GeneralSetup = 4
    }
}