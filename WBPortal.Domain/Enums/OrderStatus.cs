namespace WBSSLStore.Domain
{
    public enum OrderStatus
    {
        PENDING=0,
        ACTIVE=1,
        REJECTED=2,
        REFUNDED=3
    }

    //Pyment Process Status

    public enum PymentProcessStatus
    {
        
        COMPLETE = 1,
        FRAUDDETECTION = 2,
        REJECTED = 3
    }
}