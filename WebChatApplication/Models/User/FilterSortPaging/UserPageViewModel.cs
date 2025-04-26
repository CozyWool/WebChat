using System.ComponentModel.DataAnnotations;

namespace WebChatApplication.Models.User.FilterSortPaging;

public class UserPageViewModel(int count, int pageNumber, int pageSize)
{
    [Display(Name = "Страница")] public int PageNumber { get; } = pageNumber;
    public int TotalPages { get; } = (int) Math.Ceiling(count / (double) pageSize);


    [Display(Name = "Назад")] public bool HasPreviousPage => PageNumber > 1;
    [Display(Name = "Вперед")] public bool HasNextPage => PageNumber < TotalPages;
    [Display(Name = "Размер страницы")] public int PageSize { get; set; } = pageSize;
}