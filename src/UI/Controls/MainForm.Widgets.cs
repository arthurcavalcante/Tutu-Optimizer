using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Microsoft.Win32;
using System.Net;
using System.Net.NetworkInformation;

namespace TutusOptimizer
{
    public partial class MainForm
    {
        #region Helpers de Criação de Widgets Responsivos

        private Panel MakePageBanner(string title, string subtitle)
        {
            Panel p = new Panel();
            p.Height = 88;
            p.BackColor = theme.Card;
            p.Padding = new Padding(16, 12, 16, 12);
            p.Paint += (s, e) =>
            {
                using (Pen pen = new Pen(theme.Border, 1f))
                {
                    e.Graphics.DrawRectangle(pen, 0, 0, p.Width - 1, p.Height - 1);
                }
            };

            Label l1 = new Label();
            l1.Text = title;
            l1.Font = new Font("Segoe UI", 12f, FontStyle.Bold);
            l1.ForeColor = theme.TextPrimary;
            l1.UseMnemonic = false;
            l1.Dock = DockStyle.Top;
            l1.Height = 24;

            Label l2 = new Label();
            l2.Text = subtitle;
            l2.Font = new Font("Segoe UI", 8.5f);
            l2.ForeColor = theme.TextSecondary;
            l2.UseMnemonic = false;
            l2.Dock = DockStyle.Top;
            l2.Height = 36;

            p.Controls.Add(l2);
            p.Controls.Add(l1);
            p.Resize += (s, e) => {
                int width = Math.Max(1, p.ClientSize.Width - p.Padding.Horizontal);
                l1.Height = ResponsiveLayout.TextHeight(l1, width, 24);
                l2.Height = ResponsiveLayout.TextHeight(l2, width, 36);
                p.Height = l1.Height + l2.Height + p.Padding.Vertical;
            };
            return p;
        }

        private Panel MakeSectionHeader(string title)
        {
            Panel p = new Panel();
            p.Height = 30;
            p.BackColor = Color.Transparent;
            p.Padding = new Padding(4, 6, 4, 2);

            Label l = new Label();
            l.Text = title;
            l.Font = new Font("Segoe UI", 8f, FontStyle.Bold);
            l.ForeColor = theme.Accent;
            l.UseMnemonic = false;
            l.Dock = DockStyle.Fill;

            p.Controls.Add(l);
            return p;
        }

        private Panel MakeStatCardRow(Panel[] cards)
        {
            Panel p = new Panel();
            p.Height = 88;
            p.BackColor = Color.Transparent;
            p.Padding = new Padding(0, 2, 0, 6);

            TableLayoutPanel tbl = new TableLayoutPanel();
            tbl.Dock = DockStyle.Fill;
            tbl.RowCount = 1;
            tbl.ColumnCount = cards.Length;

            float pct = 100f / cards.Length;
            for (int i = 0; i < cards.Length; i++)
            {
                tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, pct));
                tbl.Controls.Add(cards[i], i, 0);
            }
            tbl.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

            p.Controls.Add(tbl);
            ResponsiveLayout.Grid(p, tbl, cards, cards.Length, 230, 110);
            return p;
        }

        private Panel MakeStatCard(string title, string value, Color accent)
        {
            Panel card = new Panel();
            card.Dock = DockStyle.Fill;
            card.BackColor = theme.Card;
            card.Margin = new Padding(4);
            card.Padding = new Padding(12, 10, 12, 10);
            card.Paint += (s, e) =>
            {
                using (Pen pen = new Pen(theme.Border, 1f))
                {
                    e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
                }
            };

            Label lT = new Label();
            lT.Text = title;
            lT.Font = new Font("Segoe UI", 7.5f, FontStyle.Bold);
            lT.ForeColor = accent;
            lT.UseMnemonic = false;
            lT.Dock = DockStyle.Top;
            lT.Height = 18;

            Label lV = new Label();
            lV.Text = value;
            lV.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            lV.ForeColor = theme.TextPrimary;
            lV.UseMnemonic = false;
            lV.Dock = DockStyle.Fill;

            card.Controls.Add(lV);
            card.Controls.Add(lT);
            return card;
        }

        private Panel MakeSensorMiniCard(string title, string status, string summary, Color accent)
        {
            Panel card = new Panel();
            card.Dock = DockStyle.Fill;
            card.BackColor = theme.Card;
            card.Margin = new Padding(4);
            card.Padding = new Padding(12, 8, 12, 8);
            card.Paint += (s, e) =>
            {
                using (Pen pen = new Pen(theme.Border, 1f))
                {
                    e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
                }
            };

            Label lT = new Label();
            lT.Text = title;
            lT.Font = new Font("Segoe UI", 7.5f, FontStyle.Bold);
            lT.ForeColor = accent;
            lT.UseMnemonic = false;
            lT.Dock = DockStyle.Top;
            lT.Height = 17;

            Label lS = new Label();
            lS.Text = status;
            lS.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            lS.ForeColor = theme.TextPrimary;
            lS.UseMnemonic = false;
            lS.Dock = DockStyle.Top;
            lS.Height = 22;

            Label lD = new Label();
            lD.Text = summary;
            lD.Font = new Font("Segoe UI", 7.5f);
            lD.ForeColor = theme.TextSecondary;
            lD.UseMnemonic = false;
            lD.Dock = DockStyle.Fill;

            card.Controls.Add(lD);
            card.Controls.Add(lS);
            card.Controls.Add(lT);
            return card;
        }

        private Panel MakeNoteCard(string header, string noteText)
        {
            Panel card = new Panel();
            card.Height = 52;
            card.BackColor = theme.Card;
            card.Margin = new Padding(4, 2, 4, 8);
            card.Padding = new Padding(12, 6, 12, 6);
            card.Paint += (s, e) =>
            {
                using (Pen pen = new Pen(theme.Border, 1f))
                {
                    e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
                }
            };

            Label lH = new Label();
            lH.Text = header;
            lH.Font = new Font("Segoe UI", 8.5f, FontStyle.Bold);
            lH.ForeColor = theme.TextPrimary;
            lH.Dock = DockStyle.Top;
            lH.Height = 20;

            Label lN = new Label();
            lN.Text = noteText;
            lN.Font = new Font("Segoe UI", 7.5f);
            lN.ForeColor = theme.TextSecondary;
            lN.Dock = DockStyle.Fill;

            card.Controls.Add(lN);
            card.Controls.Add(lH);
            return card;
        }

        private Panel MakeActionGrid(ActionCardDef[] actions)
        {
            int rows = (int)Math.Ceiling(actions.Length / 2.0);
            int height = rows * 130 + 8;

            Panel p = new Panel();
            p.Height = height;
            p.BackColor = Color.Transparent;
            p.Padding = new Padding(0, 2, 0, 6);

            TableLayoutPanel tbl = new TableLayoutPanel();
            tbl.Dock = DockStyle.Fill;
            tbl.ColumnCount = 2;
            tbl.RowCount = rows;
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));

            for (int r = 0; r < rows; r++)
                tbl.RowStyles.Add(new RowStyle(SizeType.Percent, 100f / rows));

            for (int i = 0; i < actions.Length; i++)
            {
                tbl.Controls.Add(MakeActionCard(actions[i]), i % 2, i / 2);
            }

            p.Controls.Add(tbl);
            Control[] cards = new Control[actions.Length];
            for (int i = 0; i < actions.Length; i++) cards[i] = tbl.Controls[i];
            ResponsiveLayout.Grid(p, tbl, cards, 2, 330, 150);
            return p;
        }

        private Panel MakeActionCard(ActionCardDef item)
        {
            Panel card = new Panel();
            card.Dock = DockStyle.Fill;
            card.BackColor = theme.Card;
            card.Margin = new Padding(4);
            card.Padding = new Padding(14, 10, 14, 10);
            card.Paint += (s, e) =>
            {
                using (Pen pen = new Pen(theme.Border, 1f))
                {
                    e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
                }
            };

            Label lT = new Label();
            lT.Text = item.Title;
            lT.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            lT.ForeColor = theme.TextPrimary;
            lT.Dock = DockStyle.Top;
            lT.Height = 22;

            Label lD = new Label();
            lD.Text = item.Desc;
            lD.Font = new Font("Segoe UI", 8.5f);
            lD.ForeColor = theme.TextSecondary;
            lD.Dock = DockStyle.Fill;

            Button btn = new AnimatedButton();
            btn.Text = item.BtnText;
            btn.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            btn.ForeColor = Color.White;
            btn.BackColor = item.AccentColor;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.Dock = DockStyle.Bottom;
            btn.Height = 32;
            btn.Cursor = Cursors.Hand;
            btn.Click += item.ClickHandler;

            card.Controls.Add(lD);
            card.Controls.Add(btn);
            card.Controls.Add(lT);
            return card;
        }

        #endregion

    }
}
