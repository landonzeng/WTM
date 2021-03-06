﻿using System;
using System.ComponentModel.DataAnnotations;
using WalkingTec.Mvvm.Core;

namespace WalkingTec.Mvvm.Mvc.Admin.ViewModels.DataPrivilegeVMs
{
    public class DataPrivilegeBatchVM : BaseBatchVM<DataPrivilege, DataPrivilege_BatchEdit>
    {
        public DataPrivilegeBatchVM()
        {
            ListVM = new DataPrivilegeListVM();
            LinkedVM = new DataPrivilege_BatchEdit();
        }

        protected override bool CheckIfCanDelete(Guid id, out string errorMessage)
        {
            errorMessage = null;
			return true;
        }
    }

	/// <summary>
    /// 批量编辑字段类
    /// </summary>
    public class DataPrivilege_BatchEdit : BaseVM
    {

    }

}
