﻿using System;
using System.Windows.Forms;

namespace EVEMon.Common.Controls
{
    public sealed class IntuitiveDataGridView : DataGridView
    {
        /// <summary>
        /// Processes F2 and Enter correctly.
        /// </summary>
        /// <param name="keyData"></param>
        /// <returns></returns>
        protected override bool ProcessDialogKey(Keys keyData)
        {
            var key = keyData & Keys.KeyCode;
            if (key == Keys.Return)
            {
                if (!IsCurrentCellInEditMode)
                    return ProcessF2Key(keyData);

                var currentCell = CurrentCell;
                CurrentCell = null;
                CurrentCell = currentCell;
                EndEdit();
                return true;
            }
            if (key == Keys.F2)
            {
                if (!IsCurrentCellInEditMode)
                    return ProcessF2Key(keyData);

                var currentCell = CurrentCell;
                CurrentCell = null;
                CurrentCell = currentCell;
                EndEdit();
                return true;
            }

            return base.ProcessDialogKey(keyData);
        }

        /// <summary>
        /// Processes F2 and Enter correctly.
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        protected override bool ProcessDataGridViewKey(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return && !IsCurrentCellInEditMode)
                return ProcessF2Key(e.KeyData);

            if (e.KeyCode == Keys.F2)
                return !IsCurrentCellInEditMode ? ProcessF2Key(e.KeyData) : EndEdit();

            return base.ProcessDataGridViewKey(e);
        }

        /// <summary>
        /// When the user clicks the cell content, we begin editing.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnCellClick(DataGridViewCellEventArgs e)
        {
            base.OnCellClick(e);

            if (e.ColumnIndex >= 0)
                BeginEdit(true);
        }
    }
}