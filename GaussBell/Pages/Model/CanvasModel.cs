using System.ComponentModel.DataAnnotations;

namespace GaussBell.Pages.Model
{
    public class CanvasModel
    {
        [Required]
        [Display(Name = "Critical Value")]
        [Range(-3, 3)]
        public double CriticalValue { get; set;}

        [Required]
        [Display(Name = "Z Value")]
        public double ZValue { get; set; }
    }
}