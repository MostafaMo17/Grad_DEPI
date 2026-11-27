using System;
using System.Collections.Generic;
using MediCare.Web.Models.Api;

namespace MediCare.Web.ViewModels;

// One page of notifications for the list page
public class NotificationsViewModel
{
    public List<NotificationDto> Items { get; set; } = new List<NotificationDto>();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public bool UnreadOnly { get; set; }

    public int TotalPages
    {
        get
        {
            if (PageSize <= 0)
            {
                return 1;
            }

            return (int)Math.Ceiling(TotalCount / (double)PageSize);
        }
    }

    public bool HasPrevious
    {
        get { return Page > 1; }
    }

    public bool HasNext
    {
        get { return Page < TotalPages; }
    }
}