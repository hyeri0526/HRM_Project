﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Oracle.ManagedDataAccess.Client;

namespace LhrMain._01코드관리
{
    /// <summary>
    ///**************************************************************************
    /// 1. Project        : 교육용 시스템
    /// 2. 업무분류       : 업무1
    /// 3. 업무명칭       : 부서코드관리
    /// 4. 최초작성 정보  : 2023.04.17 / 이혜리
    /// 5. 최근수정 정보  : 2023.04.17 / 이혜리
    ///**************************************************************************
    /// </summary>
    /// 
    public partial class 부서코드관리 : Form
    {
        #region 초기 설정
        OracleConnection con = null;
        private bool select_sw = false;
        private bool query_sw = false;
        //***************************************************************
        // 메인메뉴로부터 초기 설정값 넘겨 받기
        //***************************************************************
        public Button[] MainBtn { get; set; }           // 메인기능 버튼
        public string UserId { get; set; }              // 사용자 ID
        public Label Info_Count { get; set; }           // 검색건수
        public Label Info_Message { get; set; }         // 하단 메세지
        public string last_button_status { get; set; }  // 버튼 최종상태

        public 부서코드관리()
        {
            InitializeComponent();
            //*---Value Changed Event Handler(Start)---------------------
            ct_dept_code.TextChanged += InputData_TextChanged;
            ct_dept_name.TextChanged += InputData_TextChanged;
            ct_dept_names.TextChanged += InputData_TextChanged;
            ct_dept_seq.TextChanged += InputData_TextChanged;
            ct_dept_upp.TextChanged += InputData_TextChanged;
            ct_dept_use.CheckedChanged += InputData_TextChanged;

            //*---Validated Event Handler(Start) 유효성 검사 ------------
            ct_dept_code.Validated += Input_Validation_Check;
            ct_dept_name.Validated += Input_Validation_Check;
            ct_dept_names.Validated += Input_Validation_Check;
            ct_dept_seq.Validated += Input_Validation_Check;
        }

        private void 부서코드관리_Load(object sender, EventArgs e)
        {
            last_button_status = Utility.SetFuncBtn(MainBtn, "1");
            Utility.DataGridView_Scrolling_SpeedUp(dataGridView1);
        }
        #endregion

        #region 기능버튼(입력) Click
        //************************************************************
        //** 입력 버튼 Click 
        //************************************************************
        public void BtnInsert_Click()
        {
            var rowIdx = dataGridView1.CurrentRow == null ? 0 : dataGridView1.CurrentRow.Index;
            if (dataGridView1.Rows.Count == 0)
            {
                rowIdx = dataGridView1.Rows.Add();
            }
            else
            {
                rowIdx++;
                dataGridView1.Rows.Insert(rowIdx);
            }

            dataGridView1.Rows[rowIdx].Cells["status"].Value = "A"; // 입력상태로 초기화
            Utility.SetFocusingDataGridView(dataGridView1, rowIdx); //추가된 Row로 Focus이동
            ct_dept_code.Focus();                                   //최초 입력 컨트롤(사원번호)로 이동

            last_button_status = Utility.SetFuncBtn(MainBtn, "3");  //버튼마지막 상태: 3번 추가,삭제,저장,취소

        }
        #endregion

        #region 기능버튼(조회) Click
        //**************************************************************
        //** 조회 버튼 Click
        //**************************************************************
        public void BtnSearch_Click()
        {
            Utility.BusyIndicator(true);

            dataGridView1.Rows.Clear();
            int rowIdx = 0;
            DataGridViewRow row;
            //--DB Handling(Start)--------------------------------------
            try
            {
                con = Utility.SetOracleConnection();
                OracleCommand cmd = con.CreateCommand();
                cmd.CommandText = SQL.SQLStatement.SelectDeptCode;
                cmd.BindByName = true;                                        //바인드 변수를 사용할 때 이름으로 바인딩하도록 설정
                cmd.Parameters.Add("dept_name", OracleDbType.Varchar2).Value = txt_Search.Text + "%";
                OracleDataReader dr = cmd.ExecuteReader();
                query_sw = true;                                              // SelectionChanged Event 발생을 회피하기 위해 (On)
                while (dr.Read())
                {
                    rowIdx = dataGridView1.Rows.Add();
                    row = dataGridView1.Rows[rowIdx];
                    row.Cells["dept_code"].Value = dr["dept_code"].ToString();
                    row.Cells["dept_name"].Value = dr["dept_name"].ToString();
                    row.Cells["dept_names"].Value = dr["dept_names"].ToString(); ;
                    row.Cells["dept_seq"].Value = dr["dept_seq"].ToString(); ;
                    row.Cells["dept_upp"].Value = dr["dept_upp"].ToString(); ;
                    row.Cells["dept_use"].Value = Utility.ChangeBoolType(dr["dept_use"].ToString());

                }
                dr.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
            finally
            {
                query_sw = false; //*---SelectionChanged Event 발생을 회피하기 위해 (Off)
                Utility.BusyIndicator(false);
                if (con != null) con.Close();
            }
            //--DB Handling(End)----------------------------------------
            var recCnt = dataGridView1.RowCount;
            Info_Count.Text = recCnt.ToString();
            if (recCnt == 0)
            {
                Info_Message.Text = "조건을 만족하는 자료가 없습니다.";
                last_button_status = Utility.SetFuncBtn(MainBtn, "1");
                return;
            }

            Utility.SetFocusingDataGridView(dataGridView1, 0); //Focus를 맨 첫줄로 보내기
            this.DataList_SelectionChanged(null, null);        //선택된 첫줄을 Control에 표시하기
            last_button_status = Utility.SetFuncBtn(MainBtn, "2");
            Info_Message.Text = "자료가 정상적으로 조회 되었습니다.";
        }
        #endregion

        #region 기능버튼(수정) Click
        //************************************************************
        //** 수정 버튼 Click 
        //************************************************************
        public void BtnUpdate_Click()
        {
            MessageBox.Show(this.Name + " 수정버튼 사용하지 않음");
        }
        #endregion

        #region 기능버튼(취소) Click
        //**************************************************************
        //** 취소 버튼 Click
        //**************************************************************
        public void BtnCancel_Click()
        {
            DialogResult result = MessageBox.Show("입력 및 수정중인 자료를 취소합니다.", "취소메세지", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.No) return;

            BtnSearch_Click();
        }
        #endregion

        #region 기능버튼(삭제) Click
        //*************************************************************
        //*** 삭제 버튼 Click 
        //*************************************************************
        public void BtnDelete_Click()
        {
            if (dataGridView1.SelectedRows.Count < 1)
            {
                MessageBox.Show("삭제할 자료를 먼저 선택하세요.", "삭제확인", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            DataGridViewRow row = dataGridView1.CurrentRow;
            //신규 입력중인 자료는 단순하게 Grid에서 제거만 한다.
            if ((String)row.Cells["status"].Value == "A")
            {
                dataGridView1.Rows.RemoveAt(dataGridView1.CurrentRow.Index);
                return;
            }
            DialogResult result = MessageBox.Show(row.Cells["dept_code"].Value +
                                                  " 자료를 삭제하시겠습니까.", "삭제확인", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.No) return;

            //--DB Handling(Start)--------------------------------------
            try
            {
                con = Utility.SetOracleConnection();
                OracleCommand cmd = con.CreateCommand();
                cmd.BindByName = true;
                cmd.CommandText = SQL.SQLStatement.DeleteDeptCode;
                cmd.Parameters.Add("dept_code", OracleDbType.Varchar2).Value = row.Cells["dept_code"].Value;
                if (cmd.ExecuteNonQuery() <= 0)
                {
                    Info_Message.Text = "자료 삭제에 문제가 있습니다. 시스템 담당자에게 문의하세요.";
                    return;
                }
                dataGridView1.Rows.RemoveAt(row.Index);
                Info_Count.Text = dataGridView1.RowCount.ToString();
                Info_Message.Text = "자료가 정상적으로 삭제 되었습니다.";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
            finally
            {
                if (con != null) con.Close();
                //con?.Close();
            }
            //--DB Handling(End)-------------------------------------
            //* 삭제 후 리스트 자료가 없으면, 콘트롤의 값을 ""로 초기화
            if (dataGridView1.RowCount == 0)
            {
                select_sw = true;
                Utility.SetTextNull(data_panel);
                select_sw = false;
            }
        }
        #endregion

        #region 기능버튼(저장) Click
        //************************************************************
        //** 저장 버튼 Click (여러 건의 DATA 추가입력/수정 후 저장)
        //************************************************************
        public void BtnSave_Click()
        {
            DialogResult result = MessageBox.Show("입력 및 수정중인 자료를 저장합니다.", "저장확인", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.No) return;

            // 입력값에 오류가 있는지 여부 확인
            if (!Utility.InputErrorCheck(dataGridView1)) return;

            OracleTransaction tran = null;
            try
            {
                con = Utility.SetOracleConnection();
                tran = con.BeginTransaction(IsolationLevel.ReadCommitted); //데이터를 읽을 때 해당 데이터가 다른 트랜잭션에서 변경되지 않음
                OracleCommand cmd = con.CreateCommand();
                cmd.BindByName = true;                                     //바인드 변수를 사용할 때 이름으로 바인딩하도록 설정
                cmd.Transaction = tran;                                    //실행될 때 해당 트랜잭션 내에서 실행 여러 명령을 하나의 트랜잭션으로 묶어서 실행하고자 할 때
                foreach (DataGridViewRow row in dataGridView1.Rows)
                {
                    //MessageBox.Show(row.Cells["status"].Value.ToString());

                    if (row.Cells["status"].Value == null) continue;

                    if (row.Cells["status"].Value.Equals("A"))
                    {
                        cmd.CommandText = SQL.SQLStatement.InsertDeptCode;
                    }
                    if (row.Cells["status"].Value.Equals("U"))                                                  //* if (row.Cells["status"].Value.Equals("U"))
                    {
                        cmd.CommandText = SQL.SQLStatement.UpdateDeptCode;
                    }

                    //                   
                    cmd.Parameters.Add("dept_code", OracleDbType.Varchar2).Value = row.Cells["dept_code"].Value;
                    cmd.Parameters.Add("dept_name", OracleDbType.Varchar2).Value = row.Cells["dept_name"].Value;
                    cmd.Parameters.Add("dept_names", OracleDbType.Varchar2).Value = row.Cells["dept_names"].Value;
                    cmd.Parameters.Add("dept_seq", OracleDbType.Varchar2).Value = row.Cells["dept_seq"].Value;
                    cmd.Parameters.Add("dept_upp", OracleDbType.Varchar2).Value = row.Cells["dept_upp"].Value;
                    cmd.Parameters.Add("dept_use", OracleDbType.Varchar2).Value = Utility.ChangeBoolType(row.Cells["dept_use"].Value, "YN");

                    cmd.ExecuteNonQuery();
                    cmd.Parameters.Clear();  //*----반드시 포함
                }
                tran.Commit();
            }
            catch (Exception ex)
            {
                tran.Rollback();
                MessageBox.Show(ex.Message);
                return;
            }
            finally
            {
                if (con != null) con.Close();
            }

            // 정상적으로 저장 후 초기화하기
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (row.Cells["status"].Value == null) continue;
                row.Cells["status"].Value = "";
            }
            Info_Message.Text = "자료가 정상적으로 저장 되었습니다.";
            last_button_status = Utility.SetFuncBtn(MainBtn, "2");      //조회후 버튼 상태로 "1101001" 조회,입력, 삭제, 종료
        }
        #endregion

        #region DataGridView의 선택이 변경되었을 때 각 콘트롤에 Data Setting
        //**************************************************************
        //*** DataGridView의 선택이 변경되었을 때 각 콘트롤에 Data Setting
        //**************************************************************
        private void DataList_SelectionChanged(object sender, EventArgs e)
        {
            if (query_sw) return;  //* 조회 버튼을 누를때, 수행하지 않기 위해
            if (dataGridView1.Rows.Count == 0) return;

            select_sw = true;      //* Control의 TextChanged 이벤트와의 충돌을 피하기 위해 (On)

            Utility.SetValueToControlFromGridView(dataGridView1, data_panel);

            select_sw = false;    //* Control의 TextChanged 이벤트와의 충돌을 피하기 위해 (Off)
        }
        #endregion

        #region Conrtrol Value Changed 공통 Event Handler
        //**************************************************************
        //*** Conrtrol Value Changed 공통 Event Handler
        //**************************************************************
        private void InputData_TextChanged(object sender, EventArgs e)
        {
            if (select_sw) return; //GridView 선택 시 최초값 설정에 따른 이벤트는 무시

            if (dataGridView1.SelectedRows.Count <= 0) return;

            Utility.SetValueToGridView(dataGridView1, sender as Control);

            //*--Data Status = "수정"  설정-----------------------------
            DataGridViewRow row = dataGridView1.CurrentRow;


            if ((String)row.Cells["status"].Value == "A" || (String)row.Cells["status"].Value == "U") return;
            else
            {
                row.Cells["status"].Value = "U";
            }
            //*--Data Status = "수정"  설정-----------------------------

            last_button_status = Utility.SetFuncBtn(MainBtn, "3");
        }
        #endregion

        #region Input Data Validation Check (Validated Event)
        //************************************************************
        //** Input Data Validation Check (Validated Event)
        //************************************************************
        private void Input_Validation_Check(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count <= 0) return;

            dataGridView1.SelectedRows[0].ErrorText = "";
            //*---------------------------------------------------------------------------------------------------------
            if (ct_dept_code.Text == "")
            {
                SetError(ct_dept_code, "부서코드를 반드시 입력하세요", dataGridView1.SelectedRows[0], errorProvider1);
            }
            else
            {
                ResetError(ct_dept_code, errorProvider1);
            }

            //*---------------------------------------------------------------------------------------------------------
            if (ct_dept_names.Text == "")
            {
                SetError(ct_dept_names, "부서명(축약)을 반드시 입력하세요", dataGridView1.SelectedRows[0], errorProvider1);
            }
            else
            {
                ResetError(ct_dept_names, errorProvider1);
            }

            //*---------------------------------------------------------------------------------------------------------
            if (ct_dept_name.Text == "")
            {
                SetError(ct_dept_name, "부서명을 반드시 입력하세요", dataGridView1.SelectedRows[0], errorProvider1);
            }
            else
            {
                ResetError(ct_dept_name, errorProvider1);
            }

            //*---------------------------------------------------------------------------------------------------------
            if (ct_dept_seq.Text == "")
            {
                SetError(ct_dept_seq, "코드SEQ를 반드시 입력하세요", dataGridView1.SelectedRows[0], errorProvider1);
            }
            else
            {
                ResetError(ct_dept_seq, errorProvider1);
            }
        }
        private void SetError(Control ctl, String errMsg, DataGridViewRow row, ErrorProvider errProvider)
        {
            errProvider.SetError(ctl, errMsg);
            row.ErrorText = errMsg;
        }
        private void ResetError(Control ctl, ErrorProvider errProvider)
        {
            errProvider.SetError(ctl, "");
        }
        #endregion
    }
}
