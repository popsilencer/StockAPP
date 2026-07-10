using StockApp.Models.Dtos;
using StockApp.Models.Entities;
using StockApp.Repositories;

namespace StockApp.Services;

public class UserService
{
    private readonly UserRepository _userRepo;
    private readonly CompanyRepository _companyRepo;

    public UserService(UserRepository userRepo, CompanyRepository companyRepo)
    {
        _userRepo = userRepo;
        _companyRepo = companyRepo;
    }

    public IEnumerable<UserDto> GetAll()
        => _userRepo.GetAll().ToList().Select(ToDto);

    public UserDto? GetById(int id)
    {
        var user = _userRepo.GetById(id);
        return user == null ? null : ToDto(user);
    }

    public UserDto Create(UserCreateDto dto)
    {
        if (_userRepo.UsernameExists(dto.Username))
            throw new InvalidOperationException($"Username '{dto.Username}' already exists");

        ValidateCompany(dto.CompanyId);

        var user = new User
        {
            Username = dto.Username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            CompanyId = dto.CompanyId
        };
        _userRepo.Insert(user);
        return ToDto(user);
    }

    public UserDto Update(int id, UserUpdateDto dto)
    {
        var user = _userRepo.GetById(id);
        if (user == null)
            throw new KeyNotFoundException($"User {id} not found");

        if (_userRepo.UsernameExists(dto.Username, id))
            throw new InvalidOperationException($"Username '{dto.Username}' already exists");

        ValidateCompany(dto.CompanyId);

        user.Username = dto.Username;
        user.CompanyId = dto.CompanyId;
        _userRepo.Update(user);
        return ToDto(user);
    }

    public void ChangePassword(int id, ChangePasswordDto dto)
    {
        var user = _userRepo.GetById(id);
        if (user == null)
            throw new KeyNotFoundException($"User {id} not found");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
        _userRepo.Update(user);
    }

    public bool Delete(int id)
    {
        var user = _userRepo.GetById(id);
        if (user == null) return false;
        return _userRepo.Delete(id);
    }

    private void ValidateCompany(int? companyId)
    {
        if (companyId.HasValue && _companyRepo.GetById(companyId.Value) == null)
            throw new InvalidOperationException($"Company {companyId} not found");
    }

    private UserDto ToDto(User user)
    {
        var company = user.CompanyId.HasValue ? _companyRepo.GetById(user.CompanyId.Value) : null;
        return new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            CompanyId = user.CompanyId,
            CompanyName = company?.CompanyName
        };
    }
}
