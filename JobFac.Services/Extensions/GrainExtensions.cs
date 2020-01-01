namespace Orleans
{
    public interface IAutomaticGrainKeyMarker
    { }

    public interface IStatelessWorkerGrain : IGrainWithIntegerKey, IAutomaticGrainKeyMarker
    { }

    public interface IClusterSingletonGrain : IGrainWithIntegerKey, IAutomaticGrainKeyMarker
    { }

    public static class GrainExtensions
    {
        public static T GetGrain<T>(this IGrainFactory grainFactory) 
        where T : IGrainWithIntegerKey, IAutomaticGrainKeyMarker
            => grainFactory.GetGrain<T>(0);
    }
}
