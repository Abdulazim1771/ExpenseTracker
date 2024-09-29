﻿using ExpenseTracker.Application.Mappings;
using ExpenseTracker.Application.Requests.Common;
using ExpenseTracker.Application.Requests.Wallet;
using ExpenseTracker.Application.Stores.Interfaces;
using ExpenseTracker.Application.ViewModels.Wallet;
using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Domain.Interfaces;

namespace ExpenseTracker.Application.Stores;

internal sealed class WalletStore : IWalletStore
{
    private readonly ICommonRepository _repository;

    public WalletStore(ICommonRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public List<WalletViewModel> GetAll(GetWalletsRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var wallets = _repository.Wallets.GetAll(request.UserId);

        var viewModels = wallets
            .Select(x => x.ToViewModel())
            .ToList();

        return viewModels;
    }

    public WalletViewModel GetById(WalletRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var wallet = _repository.Wallets.GetById(request.Id, request.UserId);

        var viewModel = wallet.ToViewModel();

        return viewModel;
    }

    public WalletViewModel Create(CreateWalletRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var entity = request.ToEntity();

        var createdWallet = _repository.Wallets.Create(entity);
        _repository.SaveChanges();

        var viewModel = createdWallet.ToViewModel();

        return viewModel;
    }

    public WalletViewModel CreateDefault(Guid userId)
    {
        ArgumentNullException.ThrowIfNull(userId);

        var request = GetDefaultWallet(userId);

        return Create(request);
    }

    public WalletViewModel Update(UpdateWalletRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var entity = request.ToEntity();

        if (entity.IsMain)
        {
            var main = _repository.Wallets.GetMain(request.UserId);
            ChangeMainToSecondary(main);
        }

        _repository.Wallets.Update(entity);
        _repository.SaveChanges();

        var viewModel = entity.ToViewModel();

        return viewModel;
    }

    public void Delete(WalletRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        _repository.Wallets.Delete(request.Id, request.UserId);
        _repository.SaveChanges();
    }

    private static CreateWalletRequest GetDefaultWallet(Guid userId) => new(
        UserId: userId,
        Name: "Default Wallet",
        Description: "This is default Wallet generated by the system. You can update or delete it.",
        IsMain: true,
        Balance: 0);

    private static void ChangeMainToSecondary(Wallet? wallet)
    {
        if (wallet is not null)
        {
            wallet.IsMain = false;
        }
    }
}
