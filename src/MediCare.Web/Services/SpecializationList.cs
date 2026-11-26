using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MediCare.Web.Services;

// replace with GET /api/v1/specializations when the API has it
// These are the specializations that the API creates at startup, with their ids
public static class SpecializationList
{
    public static List<SelectListItem> GetItems()
    {
        return new List<SelectListItem>
        {
            new SelectListItem("Cardiology", "1"),
            new SelectListItem("Pediatrics", "2"),
            new SelectListItem("Dermatology", "3"),
            new SelectListItem("General Medicine", "4"),
            new SelectListItem("Orthopedics", "5"),
            new SelectListItem("Neurology", "6"),
            new SelectListItem("Ophthalmology", "7"),
            new SelectListItem("Dentistry", "8")
        };
    }
}