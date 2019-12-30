namespace Orleans
{
    public interface IClusterSingletonGrain : IGrainWithIntegerKey
    { }

    public static class ClusterSingletonGrainExtension
    {
        public static T GetGrain<T>(this IGrainFactory grainFactory) 
        where T : IClusterSingletonGrain
            => grainFactory.GetGrain<T>(0);
    }
}
