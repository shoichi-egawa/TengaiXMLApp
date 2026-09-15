using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace TengaiXMLApp
{
    public partial class Form1 : Form
    {
        private string inputXmlPath = "";
        private List<Point3D> originalPnts = new List<Point3D>();

        private Point3D ptLeftStart = null;
        private Point3D ptRightStart = null;
        private Point3D ptLeftEnd = null;
        private Point3D ptRightEnd = null;
        private int clickStep = 0;

        // 座標変換用の基準値
        private double minX, maxX, minY, maxY;
        private double centerWorldX, centerWorldY;
        private double zoomFactor = 1.0;
        private PointF panOffset = new PointF(0, 0);

        // ホイール押し込みドラッグ（パン移動）用変数
        private bool isPanning = false;
        private Point panStartMousePt;
        private PointF panStartOffset;

        public Form1()
        {
            InitializeComponent();

            // 初期フォントスタイル設定
            lblStatus.Font = new Font(lblStatus.Font, FontStyle.Regular);
            lblStatus.ForeColor = Color.Red;

            picPreview.Paint += new PaintEventHandler(PicPreview_Paint);
            picPreview.MouseClick += new MouseEventHandler(PicPreview_MouseClick);
            picPreview.MouseDown += new MouseEventHandler(PicPreview_MouseDown);
            picPreview.MouseMove += new MouseEventHandler(PicPreview_MouseMove);
            picPreview.MouseUp += new MouseEventHandler(PicPreview_MouseUp);
            picPreview.MouseWheel += new MouseEventHandler(PicPreview_MouseWheel);
            picPreview.Resize += new EventHandler(PicPreview_Resize);

            btnSelectFile.Click += new EventHandler(btnSelectFile_Click);
            btnGenerate.Click += new EventHandler(btnGenerate_Click);

            if (Controls.ContainsKey("btnReset"))
            {
                Controls["btnReset"].Click += new EventHandler(btnReset_Click);
            }
        }

        private void btnSelectFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "LandXML File (*.xml)|*.xml";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                inputXmlPath = ofd.FileName;
                lblFilePath.Text = Path.GetFileName(inputXmlPath);

                originalPnts = TengaiGenerator.ParsePntsFromXml(inputXmlPath);
                if (originalPnts == null || originalPnts.Count == 0)
                {
                    MessageBox.Show("XMLファイルから点群データ(<Pnts>)を読み込めませんでした。");
                    return;
                }

                minX = double.MaxValue; maxX = double.MinValue;
                minY = double.MaxValue; maxY = double.MinValue;
                foreach (var p in originalPnts)
                {
                    if (p.X < minX) minX = p.X; if (p.X > maxX) maxX = p.X;
                    if (p.Y < minY) minY = p.Y; if (p.Y > maxY) maxY = p.Y;
                }
                centerWorldX = (minX + maxX) / 2.0;
                centerWorldY = (minY + maxY) / 2.0;

                ResetClicks();
                zoomFactor = 1.0;
                panOffset = new PointF(0, 0);
                picPreview.Refresh();
            }
        }

        private void ResetClicks()
        {
            ptLeftStart = ptRightStart = ptLeftEnd = ptRightEnd = null;
            clickStep = 1;
            lblStatus.Text = "① オフセット距離を指定し、【左起点】を画面上でクリックしてください";
            lblStatus.ForeColor = Color.Red;
            picPreview.Refresh();
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            ResetClicks();
        }

        private void PicPreview_Resize(object sender, EventArgs e)
        {
            picPreview.Refresh();
        }

        private void PicPreview_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Middle)
            {
                isPanning = true;
                panStartMousePt = e.Location;
                panStartOffset = panOffset;
                picPreview.Cursor = Cursors.SizeAll;
            }
        }

        private void PicPreview_MouseMove(object sender, MouseEventArgs e)
        {
            if (isPanning)
            {
                panOffset.X = panStartOffset.X + (e.X - panStartMousePt.X);
                panOffset.Y = panStartOffset.Y + (e.Y - panStartMousePt.Y);
                picPreview.Refresh();
            }
        }

        private void PicPreview_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Middle)
            {
                isPanning = false;
                picPreview.Cursor = Cursors.Default;
            }
        }

        private void PicPreview_MouseWheel(object sender, MouseEventArgs e)
        {
            if (originalPnts == null || originalPnts.Count == 0) return;

            double oldZoom = zoomFactor;
            double zoomDelta = e.Delta > 0 ? 1.25 : 0.8;
            double newZoom = oldZoom * zoomDelta;

            if (newZoom < 0.1) newZoom = 0.1;
            if (newZoom > 50.0) newZoom = 50.0;

            Point mousePt = e.Location;
            float screenCenterX = picPreview.Width / 2.0f;
            float screenCenterY = picPreview.Height / 2.0f;

            panOffset.X = (float)((panOffset.X - (mousePt.X - screenCenterX)) * (newZoom / oldZoom) + (mousePt.X - screenCenterX));
            panOffset.Y = (float)((panOffset.Y - (mousePt.Y - screenCenterY)) * (newZoom / oldZoom) + (mousePt.Y - screenCenterY));

            zoomFactor = newZoom;
            picPreview.Refresh();
        }

        private void PicPreview_Paint(object sender, PaintEventArgs e)
        {
            if (originalPnts == null || originalPnts.Count == 0) return;

            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            double rangeX = maxX - minX;
            double rangeY = maxY - minY;
            if (rangeX <= 0 || rangeY <= 0) return;

            int margin = 40;
            double drawW = picPreview.Width - margin * 2;
            double drawH = picPreview.Height - margin * 2;
            if (drawW <= 0 || drawH <= 0) return;

            double baseScale = Math.Min(drawW / rangeY, drawH / rangeX);
            double scale = baseScale * zoomFactor;

            float screenCenterX = picPreview.Width / 2.0f + panOffset.X;
            float screenCenterY = picPreview.Height / 2.0f + panOffset.Y;

            foreach (var p in originalPnts)
            {
                float sx = (float)(screenCenterX + (p.Y - centerWorldY) * scale);
                float sy = (float)(screenCenterY - (p.X - centerWorldX) * scale);
                g.FillEllipse(Brushes.Black, sx - 2, sy - 2, 4, 4);
            }

            DrawSelectedPoint(g, ptLeftStart, "左起", Brushes.Red, scale, screenCenterX, screenCenterY);
            DrawSelectedPoint(g, ptRightStart, "右起", Brushes.Blue, scale, screenCenterX, screenCenterY);
            DrawSelectedPoint(g, ptLeftEnd, "左終", Brushes.Green, scale, screenCenterX, screenCenterY);
            DrawSelectedPoint(g, ptRightEnd, "右終", Brushes.Orange, scale, screenCenterX, screenCenterY);

            // --- ★ 本格方位マーク（北＝上）の描画 ---
            int cX = 40; // 左端からのX距離
            int cY = 50; // 上端からのY距離
            int r = 18;  // 円の半径

            using (Pen pLine = new Pen(Color.Cyan, 1.5f))      // 十字・外枠（水色）
            using (Pen pArrow = new Pen(Color.Red, 2f))        // 北矢印（赤）
            using (Brush bN = new SolidBrush(Color.Red))       // 「N」文字色
            using (Brush bFill = new SolidBrush(Color.Red))     // 塗りつぶし
            using (Font font = new Font("Arial", 10, FontStyle.Bold))
            {
                // 1. 外枠円と十字線
                g.DrawEllipse(pLine, cX - r, cY - r, r * 2, r * 2);
                g.DrawLine(pLine, cX, cY - r - 3, cX, cY + r + 3);       // 南北線
                g.DrawLine(pLine, cX - r - 3, cY, cX + r + 3, cY);       // 東西線

                // 2. 上向き（北）の塗りつぶし半矢印
                Point[] arrowPoly = new Point[]
                {
                    new Point(cX, cY - r - 8),       // 矢印先端（上）
                    new Point(cX - 5, cY),           // 左角
                    new Point(cX, cY)                // 中心
                };
                g.FillPolygon(bFill, arrowPoly);

                // 3. 「N」文字
                g.DrawString("N", font, bN, cX - 6, cY - r - 24);
            }
        }

        private void DrawSelectedPoint(Graphics g, Point3D p, string label, Brush brush, double scale, float screenCenterX, float screenCenterY)
        {
            if (p == null) return;
            float sx = (float)(screenCenterX + (p.Y - centerWorldY) * scale);
            float sy = (float)(screenCenterY - (p.X - centerWorldX) * scale);

            g.FillEllipse(brush, sx - 6, sy - 6, 12, 12);
            g.DrawString(label, new Font("メイリオ", 10, FontStyle.Bold), brush, sx + 8, sy - 8);
        }

        private Point3D ScreenToWorld(Point sp)
        {
            if (originalPnts == null || originalPnts.Count == 0) return null;

            int margin = 40;
            double drawW = picPreview.Width - margin * 2;
            double drawH = picPreview.Height - margin * 2;
            double baseScale = Math.Min(drawW / (maxY - minY), drawH / (maxX - minX));
            double scale = baseScale * zoomFactor;

            float screenCenterX = picPreview.Width / 2.0f + panOffset.X;
            float screenCenterY = picPreview.Height / 2.0f + panOffset.Y;

            Point3D nearest = null;
            double minDist = double.MaxValue;

            foreach (var p in originalPnts)
            {
                float sx = (float)(screenCenterX + (p.Y - centerWorldY) * scale);
                float sy = (float)(screenCenterY - (p.X - centerWorldX) * scale);
                double d = Math.Pow(sx - sp.X, 2) + Math.Pow(sy - sp.Y, 2);
                if (d < minDist)
                {
                    minDist = d;
                    nearest = p;
                }
            }
            return nearest;
        }

        private void PicPreview_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            if (originalPnts == null || originalPnts.Count == 0 || clickStep == 0) return;

            Point3D clickedPt = ScreenToWorld(e.Location);
            if (clickedPt == null) return;

            switch (clickStep)
            {
                case 1:
                    ptLeftStart = clickedPt;
                    clickStep = 2;
                    lblStatus.Text = "② 【右起点】を画面上でクリックしてください";
                    lblStatus.ForeColor = Color.Blue;
                    break;
                case 2:
                    ptRightStart = clickedPt;
                    clickStep = 3;
                    lblStatus.Text = "③ 【左終点】を画面上でクリックしてください";
                    lblStatus.ForeColor = Color.Red;
                    break;
                case 3:
                    ptLeftEnd = clickedPt;
                    clickStep = 4;
                    lblStatus.Text = "④ 【右終点】を画面上でクリックしてください";
                    lblStatus.ForeColor = Color.Blue;
                    break;
                case 4:
                    ptRightEnd = clickedPt;
                    clickStep = 0;
                    lblStatus.Text = "基準線の指定が完了しました。【拡張XML作成】を押してください。";
                    lblStatus.ForeColor = Color.DarkGreen;
                    break;
            }
            picPreview.Refresh();
        }

        private void btnGenerate_Click(object sender, EventArgs e)
        {
            if (clickStep != 0)
            {
                MessageBox.Show("画面上で起点・終点の4点を指定してください。");
                return;
            }

            if (!double.TryParse(txtOffsetLeft.Text, out double offsetLeft) ||
                !double.TryParse(txtOffsetRight.Text, out double offsetRight))
            {
                MessageBox.Show("引き延ばし距離には正しい数値を入力してください。");
                return;
            }

            // 多層設定の取得
            bool createLayers = chkMultiLayer.Checked;
            double layerInterval = 0.0;
            int layerCount = 0;

            if (createLayers)
            {
                if (!double.TryParse(txtLayerInterval.Text, out layerInterval) ||
                    !int.TryParse(txtLayerCount.Text, out layerCount) || layerCount <= 0)
                {
                    MessageBox.Show("層の設定（z値変化量・作成する層数）に正しい数値を入力してください。");
                    return;
                }
            }

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "LandXML File (*.xml)|*.xml";
            sfd.FileName = Path.GetFileNameWithoutExtension(inputXmlPath) + "_拡張板.xml";

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                bool success = TengaiGenerator.GenerateTengaiXml(
                    inputXmlPath, sfd.FileName, offsetLeft, offsetRight,
                    ptLeftStart, ptRightStart, ptLeftEnd, ptRightEnd,
                    createLayers, layerInterval, layerCount
                );

                if (success)
                {
                    MessageBox.Show("拡張板XMLの作成が完了しました！");
                }
            }
        }
    }
}