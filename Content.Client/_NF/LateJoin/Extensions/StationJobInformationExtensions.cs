using System.Linq;
using Content.Shared.GameTicking;

namespace Content.Client._NF.LateJoin;

public static class StationJobInformationExtensions
{
    public static bool IsAnyStationAvailable(IReadOnlyDictionary<NetEntity, StationJobInformation> obj)
    {
        return obj.Values.Any(station =>
            station is { IsLateJoinStation: true } &&
            (GetJobCount(station) > 0 || HasUnlimitedJobs(station))
        );
    }

    public static bool IsAnyCrewJobAvailable(IReadOnlyDictionary<NetEntity, StationJobInformation> obj)
    {
        return obj.Values.Any(station =>
            station is { IsLateJoinStation: false } &&
            (GetJobCount(station) > 0 || HasUnlimitedJobs(station))
        );
    }

    public static string GetStationNameWithJobCount(this StationJobInformation stationJobInformation)
    {
        var jobCountString = stationJobInformation.GetJobCountString();
        var stationNameWithJobCount = string.IsNullOrEmpty(jobCountString)
            ? stationJobInformation.StationName
            : stationJobInformation.StationName + jobCountString;

        return stationNameWithJobCount;
    }

    /**
     * This method returns various strings that represent the job count of a station.
     * If there are unlimited jobs available, it will return the job count followed by a "+".
     * If there are no jobs available, it will return an empty string.
     */
    public static string GetJobCountString(this StationJobInformation stationJobInformation)
    {
        var jobCount = stationJobInformation.GetJobCount();
        var hasUnlimitedJobs = stationJobInformation.HasUnlimitedJobs();
        return jobCount.WrapJobCountInParentheses(hasUnlimitedJobs);
    }

    /**
     * This method returns various strings that represent the job count of a list of stations.
     * If there are unlimited jobs available, it will return the job count followed by a "+".
     * If there are no jobs available, it will return an empty string.
     */
    public static string GetJobSumCountString(this Dictionary<NetEntity, StationJobInformation> obj)
    {
        var jobCount = obj.Values.Sum(stationJobInformation => stationJobInformation.GetJobCount());
        var hasUnlimitedJobs = obj.Values.Any(stationJobInformation => stationJobInformation.HasUnlimitedJobs());
        return jobCount.WrapJobCountInParentheses(hasUnlimitedJobs);
    }

    /**
     * One source of truth for the logic of whether a station has unlimited positions in one of its jobs.
     * This is used to determine whether to display a "+" after the job count, or not to display the job count.
     */
    public static bool HasUnlimitedJobs(this StationJobInformation stationJobInformation)
    {
        return stationJobInformation.JobsAvailable.Values.Any(count => count == null);
    }

    public static int? GetJobCount(this StationJobInformation stationJobInformation)
    {
        return stationJobInformation.JobsAvailable.Values.Sum();
    }

    public static string WrapJobCountInParentheses(this int? jobCount, bool hasUnlimitedJobs = false)
    {
        if (jobCount is null)
        {
            return "";
        }

        var jobCountString = $"{jobCount}";
        if (hasUnlimitedJobs)
        {
            jobCountString += "+";
        }
        return $" ({jobCountString})";
    }

}
