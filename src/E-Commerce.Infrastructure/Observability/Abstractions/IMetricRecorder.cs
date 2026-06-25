namespace E_Commerce.Infrastructure.Observability.Abstractions;

public interface IMetricRecorder
{
    void Increment(string name, double value = 1, params KeyValuePair<string, object>[] tags);
    void Gauge(string name, double value);
    void Histogram(string name, double value);
}