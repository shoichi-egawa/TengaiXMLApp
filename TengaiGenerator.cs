using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace TengaiXMLApp
{
    public class Point3D
    {
        public int Id { get; set; }
        public double X { get; set; } // 北座標
        public double Y { get; set; } // 東座標
        public double Z { get; set; } // 標高
        public double Station { get; set; }
    }

    public class Face3D
    {
        public int P1 { get; set; }
        public int P2 { get; set; }
        public int P3 { get; set; }
    }

    // 複数サーフェス出力用の内部データ構造
    public class SurfaceData
    {
        public string Name { get; set; }
        public List<Point3D> Points { get; set; }
        public List<Face3D> Faces { get; set; }
    }

    public static class TengaiGenerator
    {
        public static List<Point3D> ParsePntsFromXml(string xmlPath)
        {
            List<Point3D> points = new List<Point3D>();
            string content = File.ReadAllText(xmlPath);
            Match match = Regex.Match(content, @"<Pnts>(.*?)</Pnts>", RegexOptions.Singleline);
            if (!match.Success) return points;

            MatchCollection pMatches = Regex.Matches(match.Groups[1].Value, @"<P id=""(\d+)"">\s*([\d\.\-]+)\s+([\d\.\-]+)\s+([\d\.\-]+)\s*</P>");
            foreach (Match m in pMatches)
            {
                points.Add(new Point3D
                {
                    Id = int.Parse(m.Groups[1].Value),
                    X = double.Parse(m.Groups[2].Value),
                    Y = double.Parse(m.Groups[3].Value),
                    Z = double.Parse(m.Groups[4].Value)
                });
            }
            return points;
        }

        public static List<Face3D> ParseFacesFromXml(string xmlPath)
        {
            List<Face3D> faces = new List<Face3D>();
            string content = File.ReadAllText(xmlPath);
            Match match = Regex.Match(content, @"<Faces>(.*?)</Faces>", RegexOptions.Singleline);
            if (!match.Success) return faces;

            MatchCollection fMatches = Regex.Matches(match.Groups[1].Value, @"<F.*?>\s*(\d+)\s+(\d+)\s+(\d+)\s*</F>");
            foreach (Match m in fMatches)
            {
                faces.Add(new Face3D
                {
                    P1 = int.Parse(m.Groups[1].Value),
                    P2 = int.Parse(m.Groups[2].Value),
                    P3 = int.Parse(m.Groups[3].Value)
                });
            }
            return faces;
        }

        public static bool GenerateTengaiXml(
            string inputPath, string outputPath, double offsetLeft, double offsetRight,
            Point3D lStart, Point3D rStart, Point3D lEnd, Point3D rEnd,
            bool createLayers = false, double layerInterval = 0.0, int layerCount = 0)
        {
            var origPnts = ParsePntsFromXml(inputPath);
            var origFaces = ParseFacesFromXml(inputPath);
            if (origPnts.Count == 0 || origFaces.Count == 0) return false;

            // 1. 微小重複点（5mm以内）のマージ処理
            Dictionary<int, int> idMapping = new Dictionary<int, int>();
            List<Point3D> uniquePnts = new List<Point3D>();
            foreach (var p in origPnts)
            {
                var duplicate = uniquePnts.FirstOrDefault(existing => GetDist(existing, p) < 0.005);
                if (duplicate != null)
                {
                    idMapping[p.Id] = duplicate.Id;
                }
                else
                {
                    idMapping[p.Id] = p.Id;
                    uniquePnts.Add(p);
                }
            }

            foreach (var f in origFaces)
            {
                if (idMapping.ContainsKey(f.P1)) f.P1 = idMapping[f.P1];
                if (idMapping.ContainsKey(f.P2)) f.P2 = idMapping[f.P2];
                if (idMapping.ContainsKey(f.P3)) f.P3 = idMapping[f.P3];
            }

            if (idMapping.ContainsKey(lStart.Id)) lStart = uniquePnts.First(p => p.Id == idMapping[lStart.Id]);
            if (idMapping.ContainsKey(rStart.Id)) rStart = uniquePnts.First(p => p.Id == idMapping[rStart.Id]);
            if (idMapping.ContainsKey(lEnd.Id)) lEnd = uniquePnts.First(p => p.Id == idMapping[lEnd.Id]);
            if (idMapping.ContainsKey(rEnd.Id)) rEnd = uniquePnts.First(p => p.Id == idMapping[rEnd.Id]);

            var pntDict = uniquePnts.ToDictionary(p => p.Id);

            // 2. 全体メッシュグラフの構築 ＆ 境界エッジの抽出
            Dictionary<int, HashSet<int>> fullGraph = new Dictionary<int, HashSet<int>>();
            foreach (var p in uniquePnts) fullGraph[p.Id] = new HashSet<int>();

            Dictionary<string, int> edgeCount = new Dictionary<string, int>();
            void AddEdge(int a, int b)
            {
                if (a == b) return;
                string key = a < b ? $"{a}_{b}" : $"{b}_{a}";
                if (!edgeCount.ContainsKey(key)) edgeCount[key] = 0;
                edgeCount[key]++;
            }

            foreach (var f in origFaces)
            {
                if (f.P1 == f.P2 || f.P2 == f.P3 || f.P3 == f.P1) continue;

                AddEdge(f.P1, f.P2);
                AddEdge(f.P2, f.P3);
                AddEdge(f.P3, f.P1);

                if (pntDict.ContainsKey(f.P1) && pntDict.ContainsKey(f.P2)) { fullGraph[f.P1].Add(f.P2); fullGraph[f.P2].Add(f.P1); }
                if (pntDict.ContainsKey(f.P2) && pntDict.ContainsKey(f.P3)) { fullGraph[f.P2].Add(f.P3); fullGraph[f.P3].Add(f.P2); }
                if (pntDict.ContainsKey(f.P3) && pntDict.ContainsKey(f.P1)) { fullGraph[f.P3].Add(f.P1); fullGraph[f.P1].Add(f.P3); }
            }

            Dictionary<int, HashSet<int>> boundaryGraph = new Dictionary<int, HashSet<int>>();
            foreach (var p in uniquePnts) boundaryGraph[p.Id] = new HashSet<int>();

            foreach (var kvp in edgeCount)
            {
                if (kvp.Value == 1)
                {
                    string[] parts = kvp.Key.Split('_');
                    int id1 = int.Parse(parts[0]);
                    int id2 = int.Parse(parts[1]);
                    boundaryGraph[id1].Add(id2);
                    boundaryGraph[id2].Add(id1);
                }
            }

            // 3. 左右ラインの追跡
            List<Point3D> leftLine = TraceLine(lStart, lEnd, boundaryGraph, fullGraph, pntDict);
            List<Point3D> rightLine = TraceLine(rStart, rEnd, boundaryGraph, fullGraph, pntDict);

            CalculateStations(leftLine);
            CalculateStations(rightLine);

            // 4. センターラインの構築
            HashSet<int> outerIds = new HashSet<int>(leftLine.Select(p => p.Id).Concat(rightLine.Select(p => p.Id)));
            List<Point3D> centerLine = new List<Point3D>();

            var innerIds = uniquePnts.Where(p => !outerIds.Contains(p.Id)).Select(p => p.Id).ToHashSet();

            if (innerIds.Count > 0)
            {
                Dictionary<int, HashSet<int>> innerGraph = new Dictionary<int, HashSet<int>>();
                foreach (int id in innerIds)
                {
                    innerGraph[id] = fullGraph[id].Where(nId => innerIds.Contains(nId)).ToHashSet();
                }

                var candidateStarts = innerIds.Where(id => innerGraph[id].Count == 1).ToList();
                if (candidateStarts.Count == 0) candidateStarts = innerIds.ToList();

                int cStartId = candidateStarts.OrderBy(id => GetDist(pntDict[id], lStart)).First();
                int cEndId = candidateStarts.OrderBy(id => GetDist(pntDict[id], lEnd)).First();
                if (cStartId == cEndId && candidateStarts.Count > 1)
                {
                    cEndId = candidateStarts.OrderByDescending(id => GetDist(pntDict[id], lStart)).First();
                }

                centerLine = TraceCenterPath(pntDict[cStartId], pntDict[cEndId], innerGraph, pntDict);
                CalculateStations(centerLine);
            }

            // 点数一致チェック（候補フラグ）
            bool hasCenterLine = centerLine.Count > 1 && centerLine.Count == leftLine.Count && centerLine.Count == rightLine.Count;

            // 5. 点数一致チェック
            bool isValid = hasCenterLine ?
                (leftLine.Count == rightLine.Count && leftLine.Count == centerLine.Count && leftLine.Count > 1) :
                (leftLine.Count == rightLine.Count && leftLine.Count > 1);

            if (!isValid)
            {
                double errDist = DetectErrorStation(leftLine, rightLine, hasCenterLine ? centerLine : null);

                string msg = $"【点数不一致エラー】\n\n" +
                             $"・左ライン: {leftLine.Count} 点\n" +
                             $"・右ライン: {rightLine.Count} 点\n";
                if (hasCenterLine) msg += $"・中ライン: {centerLine.Count} 点\n";

                msg += $"\n起点から約 {errDist:F2} m 付近で構成が不一致です。\n確認してください。";

                MessageBox.Show(msg, "エラー", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            // 6. 断面計算と外側拡張（ベース拡張板の作成）
            int count = leftLine.Count;
            List<Point3D> tengaiPnts = new List<Point3D>();
            List<Face3D> tengaiFaces = new List<Face3D>();
            List<List<Point3D>> sections = new List<List<Point3D>>();

            int newId = 1;

            // ★ 全断面で「勾配差 0.1% (0.001)」以上があるか判定（両勾配判定）
            bool is3Point = false;
            if (hasCenterLine)
            {
                for (int i = 0; i < count; i++)
                {
                    Point3D pL = leftLine[i];
                    Point3D pR = rightLine[i];
                    Point3D pC = centerLine[i];

                    double dLC = GetDist(pL, pC);
                    double dCR = GetDist(pR, pC);

                    if (dLC > 0 && dCR > 0)
                    {
                        double slopeL = (pL.Z - pC.Z) / dLC;
                        double slopeR = (pR.Z - pC.Z) / dCR;

                        if (Math.Abs(slopeL - (-slopeR)) >= 0.001)
                        {
                            is3Point = true;
                            break;
                        }
                    }
                }
            }

            // ★ 内カーブ破綻（進行方向逆転・交差）検知フラグ
            bool hasIntersectionWarning = false;
            double warningStation = 0.0;

            for (int i = 0; i < count; i++)
            {
                Point3D pL = leftLine[i];
                Point3D pR = rightLine[i];

                double dx = pR.X - pL.X;
                double dy = pR.Y - pL.Y;
                double width = Math.Sqrt(dx * dx + dy * dy);
                if (width == 0) continue;

                double nx = dx / width;
                double ny = dy / width;

                List<Point3D> sec = new List<Point3D>();

                if (!is3Point)
                {
                    double slope = (pR.Z - pL.Z) / width;
                    sec.Add(new Point3D { Id = newId++, X = pL.X - nx * offsetLeft, Y = pL.Y - ny * offsetLeft, Z = pL.Z - slope * offsetLeft });
                    sec.Add(new Point3D { Id = newId++, X = pR.X + nx * offsetRight, Y = pR.Y + ny * offsetRight, Z = pR.Z + slope * offsetRight });
                }
                else
                {
                    Point3D pC = centerLine[i];
                    double dLC = GetDist(pL, pC);
                    double dCR = GetDist(pR, pC);
                    double slopeL = (dLC > 0) ? (pL.Z - pC.Z) / dLC : 0;
                    double slopeR = (dCR > 0) ? (pR.Z - pC.Z) / dCR : 0;

                    sec.Add(new Point3D { Id = newId++, X = pL.X - nx * offsetLeft, Y = pL.Y - ny * offsetLeft, Z = pL.Z + slopeL * offsetLeft });
                    sec.Add(new Point3D { Id = newId++, X = pC.X, Y = pC.Y, Z = pC.Z });
                    sec.Add(new Point3D { Id = newId++, X = pR.X + nx * offsetRight, Y = pR.Y + ny * offsetRight, Z = pR.Z + slopeR * offsetRight });
                }

                // ★ 内カーブでのベクトル逆転チェック（前断面との位置関係を判定）
                if (i > 0 && sections.Count > 0)
                {
                    var prevSec = sections[sections.Count - 1];

                    // 元データの進行ベクトル
                    double origDx = leftLine[i].X - leftLine[i - 1].X;
                    double origDy = leftLine[i].Y - leftLine[i - 1].Y;

                    // 拡張後の進行ベクトル（左側・右側）
                    double leftDx = sec[0].X - prevSec[0].X;
                    double leftDy = sec[0].Y - prevSec[0].Y;

                    int rIdx = sec.Count - 1;
                    double rightDx = sec[rIdx].X - prevSec[rIdx].X;
                    double rightDy = sec[rIdx].Y - prevSec[rIdx].Y;

                    // 内積によるベクトル反転チェック
                    double dotL = origDx * leftDx + origDy * leftDy;
                    double dotR = origDx * rightDx + origDy * rightDy;

                    if (dotL < 0 || dotR < 0)
                    {
                        if (!hasIntersectionWarning)
                        {
                            hasIntersectionWarning = true;
                            warningStation = leftLine[i].Station;
                        }
                    }
                }

                sections.Add(sec);
                tengaiPnts.AddRange(sec);
            }

            // ★ 内カーブ破綻が検知された場合の警告ダイアログ処理
            if (hasIntersectionWarning)
            {
                DialogResult result = MessageBox.Show(
                    $"【警告：内カーブでの拡張破綻を検知】\n\n" +
                    $"起点から約 {warningStation:F1} m 付近の急カーブ区間で、\n" +
                    $"オフセット距離が大きすぎるため拡張面が自己交差・逆転しています。\n\n" +
                    $"このままファイルを生成しますか？",
                    "内カーブ拡張警告",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning
                );

                if (result == DialogResult.No)
                {
                    return false; // 中断
                }
            }

            // 7. メッシュ（Faces）構築
            for (int i = 0; i < sections.Count - 1; i++)
            {
                var s1 = sections[i];
                var s2 = sections[i + 1];

                if (!is3Point)
                {
                    tengaiFaces.Add(new Face3D { P1 = s1[0].Id, P2 = s2[0].Id, P3 = s1[1].Id });
                    tengaiFaces.Add(new Face3D { P1 = s1[1].Id, P2 = s2[0].Id, P3 = s2[1].Id });
                }
                else
                {
                    tengaiFaces.Add(new Face3D { P1 = s1[0].Id, P2 = s2[0].Id, P3 = s1[1].Id });
                    tengaiFaces.Add(new Face3D { P1 = s1[1].Id, P2 = s2[0].Id, P3 = s2[1].Id });
                    tengaiFaces.Add(new Face3D { P1 = s1[1].Id, P2 = s2[1].Id, P3 = s1[2].Id });
                    tengaiFaces.Add(new Face3D { P1 = s1[2].Id, P2 = s2[1].Id, P3 = s2[2].Id });
                }
            }

            // 8. サーフェスリストの作成（ベース ＋ 多層オプション）
            List<SurfaceData> surfaces = new List<SurfaceData>();
            surfaces.Add(new SurfaceData { Name = "拡張板", Points = tengaiPnts, Faces = tengaiFaces });

            if (createLayers && layerCount > 0 && layerInterval != 0.0)
            {
                for (int i = 1; i <= layerCount; i++)
                {
                    double zOffset = layerInterval * i;
                    string layerName = layerInterval < 0 ? $"-{i}s" : $"+{i}s";

                    List<Point3D> layerPnts = new List<Point3D>();
                    foreach (var p in tengaiPnts)
                    {
                        layerPnts.Add(new Point3D
                        {
                            Id = p.Id,
                            X = p.X,
                            Y = p.Y,
                            Z = p.Z + zOffset
                        });
                    }

                    surfaces.Add(new SurfaceData
                    {
                        Name = layerName,
                        Points = layerPnts,
                        Faces = tengaiFaces
                    });
                }
            }

            SaveLandXml(outputPath, surfaces);
            return true;
        }

        private static List<Point3D> TraceLine(
            Point3D start, Point3D end,
            Dictionary<int, HashSet<int>> boundaryGraph,
            Dictionary<int, HashSet<int>> fullGraph,
            Dictionary<int, Point3D> pntDict)
        {
            List<Point3D> path = new List<Point3D> { start };
            HashSet<int> visited = new HashSet<int> { start.Id };

            Point3D current = start;

            while (current.Id != end.Id)
            {
                var neighbors = boundaryGraph[current.Id]
                    .Where(id => !visited.Contains(id))
                    .Select(id => pntDict[id])
                    .ToList();

                if (neighbors.Count == 0)
                {
                    neighbors = fullGraph[current.Id]
                        .Where(id => !visited.Contains(id))
                        .Select(id => pntDict[id])
                        .ToList();
                }

                if (neighbors.Count == 0) break;

                if (neighbors.Any(n => n.Id == end.Id))
                {
                    path.Add(pntDict[end.Id]);
                    break;
                }

                Point3D next = neighbors.OrderBy(n => GetDist(n, end)).First();

                visited.Add(next.Id);
                path.Add(next);
                current = next;

                if (path.Count > 10000) break;
            }

            return path;
        }

        private static List<Point3D> TraceCenterPath(
            Point3D start, Point3D end, Dictionary<int, HashSet<int>> innerGraph,
            Dictionary<int, Point3D> pntDict)
        {
            List<Point3D> path = new List<Point3D> { start };
            HashSet<int> visited = new HashSet<int> { start.Id };

            Point3D current = start;

            while (current.Id != end.Id)
            {
                if (!innerGraph.ContainsKey(current.Id)) break;

                var neighbors = innerGraph[current.Id]
                    .Where(id => !visited.Contains(id))
                    .Select(id => pntDict[id])
                    .ToList();

                if (neighbors.Count == 0) break;

                if (neighbors.Any(n => n.Id == end.Id))
                {
                    path.Add(pntDict[end.Id]);
                    break;
                }

                Point3D next = neighbors.OrderBy(n => GetDist(n, end)).First();
                visited.Add(next.Id);
                path.Add(next);
                current = next;

                if (path.Count > 10000) break;
            }

            return path;
        }

        private static void CalculateStations(List<Point3D> line)
        {
            if (line == null || line.Count == 0) return;
            line[0].Station = 0.0;
            for (int i = 1; i < line.Count; i++)
            {
                line[i].Station = line[i - 1].Station + GetDist(line[i - 1], line[i]);
            }
        }

        private static double DetectErrorStation(List<Point3D> left, List<Point3D> right, List<Point3D> center)
        {
            int minCount = Math.Min(left.Count, right.Count);
            if (center != null) minCount = Math.Min(minCount, center.Count);

            for (int i = 0; i < minCount; i++)
            {
                double diff = Math.Abs(left[i].Station - right[i].Station);
                if (diff > 0.8) return left[i].Station;
            }
            return minCount > 0 ? left[minCount - 1].Station : 0.0;
        }

        private static double GetDist(Point3D p1, Point3D p2)
        {
            double dx = p2.X - p1.X;
            double dy = p2.Y - p1.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        private static void SaveLandXml(string path, List<SurfaceData> surfaces)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            sb.AppendLine("<LandXML xmlns=\"http://www.landxml.org/schema/LandXML-1.2\" version=\"1.2\">");
            sb.AppendLine("  <Surfaces>");
            foreach (var surf in surfaces)
            {
                sb.AppendLine($"    <Surface name=\"{surf.Name}\">");
                sb.AppendLine("      <Definition surfType=\"TIN\">");
                sb.AppendLine("        <Pnts>");
                foreach (var p in surf.Points)
                {
                    sb.AppendLine($"          <P id=\"{p.Id}\">{p.X:F8} {p.Y:F8} {p.Z:F8}</P>");
                }
                sb.AppendLine("        </Pnts>");
                sb.AppendLine("        <Faces>");
                foreach (var f in surf.Faces)
                {
                    sb.AppendLine($"          <F>{f.P1} {f.P2} {f.P3}</F>");
                }
                sb.AppendLine("        </Faces>");
                sb.AppendLine("      </Definition>");
                sb.AppendLine("    </Surface>");
            }
            sb.AppendLine("  </Surfaces>");
            sb.AppendLine("</LandXML>");

            File.WriteAllText(path, sb.ToString(), Encoding.UTF8);
        }
    }
}