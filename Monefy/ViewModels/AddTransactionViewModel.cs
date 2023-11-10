﻿using Monefy.Enums;
using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Monefy.Services.Classes;
using System.DirectoryServices;
using Monefy.Services.Interfaces;
using Monefy.Models;

namespace Monefy.ViewModels;

public class AddTransactionViewModel : ViewModelBase, INotifyPropertyChanged
{
    private string _description;
    private string _amount;
    private bool _isExpenseChecked;
    private bool _isIncomeChecked;
    private string? _selectedCategory;
    private List<string> _availableCategories;

    private readonly INavigationService _navigationService;
    private readonly ITransactionsService _transactionsService;

    public event PropertyChangedEventHandler PropertyChanged;


    public string Description
    {
        get => _description;
        set => Set(ref _description, value);
    }

    public string Amount
    {
        get => _amount;
        set => Set(ref _amount, value);
    }


    public bool IsExpenseChecked
    {
        get => _isExpenseChecked; 
        set
        {
            if (_isExpenseChecked != value)
            {
                _isExpenseChecked = value;
                OnPropertyChanged(nameof(IsExpenseChecked));
                OnPropertyChanged(nameof(IsIncomeChecked));

                UpdateAvailableCategories();
            }
        }
    }

    public bool IsIncomeChecked
    {
        get { return _isIncomeChecked; }
        set
        {
            if (_isIncomeChecked != value)
            {
                _isIncomeChecked = value;
                OnPropertyChanged(nameof(IsIncomeChecked));
                OnPropertyChanged(nameof(IsExpenseChecked));

                UpdateAvailableCategories();
            }
        }
    }

    public string SelectedCategory
    {
        get { return _selectedCategory; }
        set
        {
            if (_selectedCategory != value)
            {
                _selectedCategory = value;
                OnPropertyChanged(nameof(SelectedCategory));
            }
        }
    }

    public List<string> AvailableCategories
    {
        get { return _availableCategories; }
        set
        {
            if (_availableCategories != value)
            {
                _availableCategories = value;
                OnPropertyChanged(nameof(AvailableCategories));
            }
        }
    }

    public AddTransactionViewModel(INavigationService navigationService, ITransactionsService transactionsService)
    {
        _navigationService = navigationService;
        _transactionsService = transactionsService;

        UpdateAvailableCategories();
    }

    private void UpdateAvailableCategories()
    {
        if (IsExpenseChecked)
        {
            AvailableCategories = Enum.GetValues(typeof(ExpenseCategory))
                       .Cast<ExpenseCategory>()
                       .Select(category => category.ToString())
                       .ToList();
        }
        else if (IsIncomeChecked)
        {
            AvailableCategories = Enum.GetValues(typeof(IncomeCategory))
                       .Cast<IncomeCategory>()
                       .Select(category => category.ToString())
                       .ToList();
        }
        else
        {
            AvailableCategories = new List<string>();
        }

        SelectedCategory = null;
    }

    public ButtonCommand AddTransactionCommand
    {
        get => new(
        () =>
        {
            if (IsIncomeChecked)
            {
                IncomeCategory incomeCategory;
                Enum.TryParse(SelectedCategory, out incomeCategory);

                _transactionsService.AddTransaction(
                    new Transaction(
                        description: Description,
                        amount: Convert.ToDouble(Amount),
                        category: incomeCategory
                    )
                );
            }
            else if (IsExpenseChecked)
            {
                ExpenseCategory expenseCategory;
                Enum.TryParse(SelectedCategory, out expenseCategory);

                _transactionsService.AddTransaction(
                    new Transaction(
                        description: Description,
                        amount: Convert.ToDouble(Amount),
                        category: expenseCategory
                    )
                );
            }

            Description = null;
            Amount = null;
            IsIncomeChecked = false;
            IsExpenseChecked = false;
            SelectedCategory = null;

            _navigationService.NavigateTo<HomeViewModel>();
        },
        () =>
        {
            return 
                !string.IsNullOrEmpty( Description ) &&
                Convert.ToDouble(Amount) > 0 &&
                (IsExpenseChecked || IsIncomeChecked) &&
                !string.IsNullOrEmpty(SelectedCategory);
        }
        );
    }

    public ButtonCommand CancelCommand
    {
        get => new(() =>
        {
            Description = null;
            Amount = null;
            IsIncomeChecked = false;
            IsExpenseChecked = false;
            SelectedCategory = null;

            _navigationService.NavigateTo<HomeViewModel>();
        });
    }


    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
