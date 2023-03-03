﻿using System.Collections.Generic;
using System.Linq;
using Accord.Collections;
using Sailfish.Presentation;

namespace Sailfish.Analysis;

internal class TrackingFileFinder : ITrackingFileFinder
{
    private readonly ITrackingFileDirectoryReader trackingFileDirectoryReader;

    public TrackingFileFinder(ITrackingFileDirectoryReader trackingFileDirectoryReader)
    {
        this.trackingFileDirectoryReader = trackingFileDirectoryReader;
    }

    public BeforeAndAfterTrackingFiles GetBeforeAndAfterTrackingFiles(string directory, string beforeTarget, OrderedDictionary<string, string> tags)
    {
        var files = trackingFileDirectoryReader.FindTrackingFilesInDirectoryOrderedByLastModified(directory);
        //
        // string? beforeTargetOverride = null;
        // if (!string.IsNullOrEmpty(beforeTarget) && !string.IsNullOrWhiteSpace(beforeTarget))
        // {
        //     beforeTargetOverride = files.Select(Path.GetFileName).SingleOrDefault(x => x?.ToLowerInvariant() == beforeTarget);
        //     if (beforeTargetOverride is null)
        //     {
        //         throw new SailfishException("The file name provided for the before target was not found");
        //     }
        // }

        if (tags.Any())
        {
            var joinedTags = DefaultFileSettings.JoinTags(tags); // empty string
            files = files.Where(x => x.Replace(DefaultFileSettings.TrackingSuffix, string.Empty).EndsWith(joinedTags)).ToList();
        }
        else
        {
            files = files.Where(x => !x.Contains(DefaultFileSettings.TagsPrefix)).ToList();
        }

        return files.Count < 2
            ? new BeforeAndAfterTrackingFiles(new List<string>(), new List<string>())
            : new BeforeAndAfterTrackingFiles(new List<string> { files[1] }, new List<string>() { files[0] });
    }
}