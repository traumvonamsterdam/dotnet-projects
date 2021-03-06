﻿using Burgler.Entities.OrderNS;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Burgler.BusinessLogic.OrderLogic
{
    public interface IOrderServices
    {
        Task<OrderDto> GetOrder(Guid Id);
        Task<List<OrderDto>> GetPendingOrders();
        Task<List<OrderDto>> GetPlacedOrders();
        Task CreateOrder(CreateCommand command);
        Task EditOrder(EditCommand command);
        Task ChangeOrderStatus(ChangeStatusCommand command, Guid id);
        Task StaffUpdateOrder(StaffUpdateCommand command, Guid id);
        Task DeleteOrder(Guid Id);
    }
}
