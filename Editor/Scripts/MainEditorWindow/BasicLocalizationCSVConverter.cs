using System.Collections.Generic;
using Mono.Csv;

namespace m039.BasicLocalization
{
    public static class BasicLocalizationCSVConverter
    {
        static internal void Export(BasicLocalizationProfile localizationProfile, string path)
        {
            if (!string.IsNullOrEmpty(path))
            {
                var exportData = localizationProfile.Editor.GetSnapshotData();

                using (var writer = new CsvFileWriter(path))
                {
                    var row = new List<string>();

                    // Write header.
                    row.Add("Key");
                    row.AddRange(exportData.languages);
                    writer.WriteRow(row);

                    // Write rest translations.
                    for (int i = 0; i < exportData.keys.Count; i++)
                    {
                        row.Clear();
                        row.Add(exportData.keys[i]);
                        row.AddRange(exportData.translations[i]);
                        writer.WriteRow(row);
                    }
                }
            }
        }

        static internal void Import(BasicLocalizationProfile localizationProfile, string path)
        {
            if (!string.IsNullOrEmpty(path))
            {
                var snapshotData = new BasicLocalizationProfile.SnapshotData();

                using (var reader = new CsvFileReader(path))
                {
                    var row = new List<string>();

                    // Read languages.

                    if (reader.ReadRow(row))
                    {
                        if (row.Count > 1)
                        {
                            for (int i = 1; i < row.Count; i++)
                            {
                                snapshotData.languages.Add(row[i]);
                            }
                        }
                    }

                    // Read rest translations.

                    while (reader.ReadRow(row))
                    {
                        if (row.Count > 0)
                        {
                            snapshotData.keys.Add(row[0]);
                        }

                        if (row.Count > 1)
                        {
                            var trs = new List<string>();
                            for (int i = 1; i < row.Count; i++)
                            {
                                trs.Add(row[i]);
                            }
                            snapshotData.translations.Add(trs);
                        }
                    }
                }

                localizationProfile.Editor.ImportSnapshotData(snapshotData);
            }
        }
    }
}
