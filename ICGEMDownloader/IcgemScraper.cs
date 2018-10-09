using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using IronWebScraper;

namespace ICGEMDownloader
{
    class IcgemScraper : WebScraper
    {
        public IList<StaticModel> StaticModels { get; set; }

        public override void Parse(Response response)
        {
            foreach (var row in response.Css("table>tbody>tr.tom-row,tr.tom-row-odd"))
            {
                if (row.Css("td.tom-cell-modelfile").First().CssExists("a:contains(zip)"))
                {
                    StaticModels.Add(new StaticModel
                    {
                        Name = row.Css("td.tom-cell-name").First().InnerTextClean,
                        Data = row.Css("td.tom-cell-data").First().InnerTextClean,
                        Degree = Convert.ToInt32(row.Css("td.tom-cell-degree").First().InnerTextClean),
                        Year = Convert.ToInt32(row.Css("td.tom-cell-year").First().InnerTextClean),
                        DownLink = row.Css("td.tom-cell-modelfile").First().Css("a:contains(zip)")?.First()
                            .GetAttribute("href")
                    });               
                }
            }
        }

        public override void Init()
        {
            StaticModels = new List<StaticModel>();
            LoggingLevel = LogLevel.All;
            Request("http://icgem.gfz-potsdam.de/tom_longtime", Parse);
        }
    }
}
