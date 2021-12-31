//
// Copyright (C) 2013-2022 getMaNGOS <https://getmangos.eu>
//
// This program is free software. You can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation. either version 2 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY. Without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program. If not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
//

using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace Mangos.DBCReader;

public partial class frmMain
{
    public frmMain()
    {
        InitializeComponent();
        _cmdBrowse.Name = "cmdBrowse";
        _DBCData.Name = "DBCData";
        _txtQuery.Name = "txtQuery";
        _cmdSearch.Name = "cmdSearch";
    }

    private readonly List<int> IsFloat = new();
    private readonly List<int> IsString = new();
    private byte[] StringData = Array.Empty<byte>();

    private void cmdBrowse_Click(object sender, EventArgs e)
    {
        OpenFileDialog fdlg = new()
        {
            Title = "Which DBC You Want to View",
            Filter = "DBC File (*.dbc)|*.dbc",
            FilterIndex = 2,
            RestoreDirectory = true
        };
        if (fdlg.ShowDialog() == DialogResult.OK)
        {
            txtFile.Text = fdlg.FileName;
            IsFloat.Clear();
            IsString.Clear();
            DBCData.Clear();
            cmbColumn.Items.Clear();
            FileStream fs = new(fdlg.FileName, FileMode.Open, FileAccess.Read);
            BinaryReader s = new(fs);
            s.BaseStream.Seek(0L, SeekOrigin.Begin);
            var Buffer = s.ReadBytes((int)FileSystem.FileLen(fdlg.FileName));
            HandleDBCData(Buffer);
            s.Close();
        }
    }

    private void HandleDBCData(byte[] Data)
    {
        var DBCType = Conversions.ToString((char)Data[0]) + (char)Data[1] + (char)Data[2] + (char)Data[3];
        var Rows = BitConverter.ToInt32(Data, 4);
        var Columns = BitConverter.ToInt32(Data, 8);
        var RowLength = BitConverter.ToInt32(Data, 12);
        var StringPartLength = BitConverter.ToInt32(Data, 16);
        if (DBCType != "WDBC")
        {
            MessageBox.Show("This file is not a DBC file.", "Error");
            return;
        }

        if (Rows <= 0 || Columns <= 0 || RowLength <= 0)
        {
            MessageBox.Show("This file is not a DBC file.", "Error");
            return;
        }

        int i;
        int j;
        int tmpOffset;
        var tmpStr = new string[Columns];
        var AmtZero = new int[Columns];
        List<int>[] foundStrings = new List<int>[Columns];
        var loopTo = Columns - 1;
        for (i = 0; i <= loopTo; i++)
        {
            ColumnHeader tmpColumn = new()
            {
                Text = i.ToString(),
                Width = 90
            };
            DBCData.Columns.Add(tmpColumn);
            cmbColumn.Items.Add(i.ToString());
            AmtZero[i] = 0;
            foundStrings[i] = new List<int>();
        }

        // Check if any column uses floats instead of uint32's
        // Code below doesn't work at the moment, flags are in some cases counted as floats
        float tmpSng;
        var tmpString = "";
        List<int> notFloat = new();
        for (i = 0; i <= 99; i++)
        {
            if (i > Rows - 1)
            {
                break;
            }

            var loopTo1 = Columns - 1;
            for (j = 0; j <= loopTo1; j++)
            {
                if (notFloat.Contains(j) == false)
                {
                    tmpOffset = 20 + (i * RowLength) + (j * 4);
                    tmpSng = Math.Abs(BitConverter.ToSingle(Data, tmpOffset));
                    if (tmpSng < 50000f) // Only allow floats to be between 0 and 50000 (negative and positive)
                    {
                        tmpString = tmpSng.ToString().Replace(",", ".");
                        tmpString = tmpString[(tmpString.IndexOf(".") + 1)..];
                        if (!tmpSng.ToString().Replace(",", ".").Contains(".", StringComparison.CurrentCulture) || tmpString.Length >= 1 && tmpString.Length <= 6) // Only allow a minimum of 1 decimal and a maximum of 5 decimals
                        {
                            if (IsFloat.Contains(j) == false)
                            {
                                IsFloat.Add(j);
                            }
                        }
                        else if (IsFloat.Contains(j))
                        {
                            IsFloat.Remove(j);
                            notFloat.Add(j);
                        }
                    }
                }
            }
        }

        // Check if any column is a string
        int tmpInt;
        List<int> notString = new();
        if (StringPartLength > 0)
        {
            for (i = 0; i <= 99; i++)
            {
                if (i > Rows - 1)
                {
                    break;
                }

                var loopTo2 = Columns - 1;
                for (j = 0; j <= loopTo2; j++)
                {
                    if (notString.Contains(j) == false && IsFloat.Contains(j) == false)
                    {
                        tmpOffset = 20 + (i * RowLength) + (j * 4);
                        tmpInt = BitConverter.ToInt32(Data, tmpOffset);
                        if (tmpInt >= 0 && tmpInt < StringPartLength)
                        {
                            tmpOffset = 20 + (Rows * RowLength) + tmpInt;
                            if (tmpInt > 0 && Data[tmpOffset - 1] > 0)
                            {
                                if (IsString.Contains(j))
                                {
                                    IsString.Remove(j);
                                }

                                notString.Add(j);
                                continue;
                            }

                            if (tmpInt > 0)
                            {
                                tmpString = GetString(ref Data, tmpOffset);
                            }
                            else
                            {
                                AmtZero[j] += 1;
                            }

                            if (tmpInt == 0 || IsValidString(tmpString))
                            {
                                if (IsString.Contains(j) == false)
                                {
                                    IsString.Add(j);
                                }

                                if (foundStrings[j].Contains(tmpInt) == false)
                                {
                                    foundStrings[j].Add(tmpInt);
                                }
                            }
                            else if (IsString.Contains(j))
                            {
                                IsString.Remove(j);
                                notString.Add(j);
                            }
                        }
                        else if (IsString.Contains(j))
                        {
                            IsString.Remove(j);
                            notString.Add(j);
                        }
                    }
                }
            }
        }

        var loopTo3 = Columns - 1;
        for (i = 0; i <= loopTo3; i++)
        {
            if (IsString.Contains(i))
            {
                if (IsString.Contains(i) && AmtZero[i] > 90)
                {
                    IsString.Remove(i);
                    continue;
                }

                if (Conversions.ToBoolean(Operators.ConditionalCompareObjectLess(foundStrings[i].Count, Interaction.IIf(Rows < 10, 2, 5), false)))
                {
                    IsString.Remove(i);
                }
            }
        }

        Application.DoEvents();
        var tmpTag = new int[Columns];
        var loopTo4 = Rows - 1;
        for (i = 0; i <= loopTo4; i++)
        {
            var loopTo5 = Columns - 1;
            for (j = 0; j <= loopTo5; j++)
            {
                tmpTag[j] = 0;
                tmpOffset = 20 + (i * RowLength) + (j * 4);
                if (IsFloat.Contains(j))
                {
                    tmpStr[j] = BitConverter.ToSingle(Data, tmpOffset).ToString();
                }
                else if (IsString.Contains(j))
                {
                    tmpOffset = BitConverter.ToInt32(Data, tmpOffset);
                    tmpTag[j] = tmpOffset;
                    tmpStr[j] = GetString(ref Data, 20 + (Rows * RowLength) + tmpOffset);
                }
                else
                {
                    tmpStr[j] = BitConverter.ToInt32(Data, tmpOffset).ToString();
                }
            }

            {
                var withBlock = DBCData.Items.Add(tmpStr[0]);
                if (Columns > 1)
                {
                    var loopTo6 = Columns - 1;
                    for (j = 1; j <= loopTo6; j++)
                    {
                        withBlock.SubItems.Add(tmpStr[j]).Tag = tmpTag[j];
                    }
                }
            }

            if (i % 100 == 0)
            {
                ProgressBar.Value = (int)((i + 1) / (double)Rows * 100d);
            }
        }

        StringData = new byte[StringPartLength];
        Array.Copy(Data, 20 + (Rows * RowLength), StringData, 0, StringData.Length);
        ProgressBar.Value = 0;
    }

    private bool IsValidString(string str)
    {
        var chars = str.ToCharArray();
        var accepted = @" ():.,'-*_?\/<>;$%".ToCharArray();
        for (int i = 0, loopTo = chars.Length - 1; i <= loopTo; i++)
        {
            if (chars[i] is (< 'A' or > 'z') and (< '0' or > '9'))
            {
                for (int j = 0, loopTo1 = accepted.Length - 1; j <= loopTo1; j++)
                {
                    if (chars[i] == accepted[j])
                    {
                        break;
                    }

                    if (j == accepted.Length - 1)
                    {
                        return false;
                    }
                }
            }
        }

        return true;
    }

    private void DBCData_ColumnClick(object sender, ColumnClickEventArgs e)
    {
        int i;
        int tmpInt;
        float tmpSng;
        var Buffer = new byte[4];
        bool A_Float;
        bool A_String;
        var FailString = false;
        var DoneChange = false;
        int tmpInt2;
        if (DBCData.Items.Count == 0)
        {
            return;
        }

        A_Float = IsFloat.Contains(e.Column);
        A_String = A_Float == false && IsString.Contains(e.Column);
        var loopTo = DBCData.Items.Count - 1;
        for (i = 0; i <= loopTo; i++)
        {
            if (A_Float) // To string or int if it's not possible
            {
                tmpSng = Conversions.ToSingle(DBCData.Items[i].SubItems[e.Column].Text);
                Buffer = BitConverter.GetBytes(tmpSng);
                tmpInt = BitConverter.ToInt32(Buffer, 0);
                if (FailString == false && tmpInt > 0 && tmpInt < StringData.Length - 4)
                {
                    DBCData.Items[i].SubItems[e.Column].Text = GetString(ref StringData, tmpInt);
                    DBCData.Items[i].SubItems[e.Column].Tag = tmpInt;
                }
                else
                {
                    if (DoneChange == false)
                    {
                        DoneChange = true;
                        FailString = true;
                        for (int j = 0, loopTo1 = i - 1; j <= loopTo1; j++)
                        {
                            tmpInt2 = Conversions.ToInteger(Conversions.ToString(DBCData.Items[j].SubItems[e.Column].Tag));
                            DBCData.Items[j].SubItems[e.Column].Tag = 0;
                            DBCData.Items[j].SubItems[e.Column].Text = tmpInt2.ToString();
                        }

                        IsFloat.Remove(e.Column);
                        IsString.Add(e.Column);
                    }

                    DBCData.Items[i].SubItems[e.Column].Text = tmpInt.ToString();
                }
            }
            else if (A_String) // To int
            {
                tmpInt = Conversions.ToInteger(Conversions.ToString(DBCData.Items[i].SubItems[e.Column].Tag));
                DBCData.Items[i].SubItems[e.Column].Tag = 0;
                DBCData.Items[i].SubItems[e.Column].Text = tmpInt.ToString();
            }
            else // To float
            {
                tmpInt = Conversions.ToInteger(DBCData.Items[i].SubItems[e.Column].Text);
                Buffer = BitConverter.GetBytes(tmpInt);
                DBCData.Items[i].SubItems[e.Column].Text = BitConverter.ToSingle(Buffer, 0).ToString();
            }

            ProgressBar.Value = (int)((i + 1) / (double)DBCData.Items.Count * 100d);
        }

        if (FailString)
        {
            A_Float = false;
            A_String = true;
        }

        if (A_Float)
        {
            IsFloat.Remove(e.Column);
            IsString.Add(e.Column);
        }
        else if (A_String)
        {
            IsString.Remove(e.Column);
        }
        else
        {
            IsFloat.Add(e.Column);
        }

        ProgressBar.Value = 0;
    }

    private string GetString(ref byte[] Data, int Index)
    {
        int i;
        var loopTo = Data.Length - 1;
        for (i = Index; i <= loopTo; i++)
        {
            if (Data[i] == 0)
            {
                break;
            }
        }

        return i == Index ? "" : Encoding.ASCII.GetString(Data, Index, i - Index);
    }

    private void cmdSearch_Click(object sender, EventArgs e)
    {
        if (cmbColumn.Items.Count == 0)
        {
            MessageBox.Show("To be able to search you must first open up a DBC File!", "Unable to search");
            return;
        }

        if (cmbColumn.SelectedItem is null)
        {
            MessageBox.Show("You must select a column to search in first!", "Unable to search");
            return;
        }

        var sQuery = txtQuery.Text;
        var column = Conversions.ToInteger(cmbColumn.SelectedItem);
        var start = 0;
        if (DBCData.SelectedItems.Count > 0)
        {
            start = int.MaxValue - 1;
            foreach (ListViewItem item in DBCData.SelectedItems)
            {
                if (item.Index < start)
                {
                    start = item.Index;
                }
            }

            start += 1;
        }

        var is_string = IsString.Contains(column);
        for (int i = start, loopTo = DBCData.Items.Count - 1; i <= loopTo; i++)
        {
            var sItem = DBCData.Items[i].SubItems[column].Text;
            if (is_string && sItem.Contains(sQuery, StringComparison.OrdinalIgnoreCase))
            {
                DBCData.SelectedItems.Clear();
                DBCData.Items[i].Selected = true;
                DBCData.Items[i].EnsureVisible();
                return;
            }

            if ((sItem ?? "") == (sQuery ?? ""))
            {
                DBCData.SelectedItems.Clear();
                DBCData.Items[i].Selected = true;
                DBCData.Items[i].EnsureVisible();
                return;
            }
        }

        MessageBox.Show("No result for that search was found!" + Constants.vbCrLf + Constants.vbCrLf + "Do note that the search starts from your current selection.", "No result found");
    }

    private void txtQuery_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Enter)
        {
            cmdSearch.PerformClick();
        }
    }
}
