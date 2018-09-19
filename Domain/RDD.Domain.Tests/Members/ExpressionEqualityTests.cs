using RDD.Domain.Helpers;
using RDD.Domain.Helpers.Expressions.Equality;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using Xunit;

namespace RDD.Domain.Tests.Members
{
    public class ExpressionEqualityTests
    {
        public class Address
        {
            public string Suburb { get; set; }
            public int Postcode { get; set; }

            public override bool Equals(object obj)
            {
                var other = obj as Address;
                if (other == null) return false;
                return Suburb.Equals(other.Suburb) && Postcode.Equals(other.Postcode);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    var hash = 17;
                    hash = hash * 23 + Suburb.GetHashCode();
                    hash = hash * 23 + Postcode.GetHashCode();
                    return hash;
                }
            }
        }
        public class Customer
        {
            public string Name { get; set; }
            public Address Address { get; set; }

            public override bool Equals(object obj)
            {
                var other = obj as Customer;
                if (other == null) return false;
                return Name.Equals(other.Name) && ((Address == null && other.Address == null) || (Address?.Equals(other.Address) ?? false));
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    var hash = 17;
                    hash = hash * 23 + Name.GetHashCode();
                    hash = hash * 23 + Address.GetHashCode();
                    return hash;
                }
            }
        }
        public class Order
        {
            public int Number { get; set; }
            public Customer Customer { get; set; }

            public IEnumerable<OrderLineItem> LineItems { get; set; }
        }
        public class OrderLineItem
        {
            public Product Product { get; set; }
            public int Quantity { get; set; }
        }
        public class Product
        {
            public string Name { get; set; }
            public decimal Price { get; set; }
        }

        public class ExpressionEqualityComparerTests
        {
            private readonly ExpressionEqualityComparer _sut;

            public ExpressionEqualityComparerTests()
            {
                _sut = new ExpressionEqualityComparer();
            }

            [Fact]
            public void Equals_Same1_AreEqual()
            {
                Expression<Func<Order, object>> x = order => order.Customer.Address;
                Expression<Func<Order, object>> y = order => order.Customer.Address;
                var e = _sut.Equals(x, y);
                Assert.True(e);
            }

            [Fact]
            public void Equals_Same2_AreEqual()
            {
                Expression<Func<Order, bool>> x = order => order.Number == 5;
                Expression<Func<Order, bool>> y = order => order.Number == 5;
                var e = _sut.Equals(x, y);
                Assert.True(e);
            }

            [Fact]
            public void Equals_Same3_AreEqual()
            {
                var customer = new Customer { Name = "john" };
                Expression<Func<Order, bool>> x = order => order.Customer == customer;
                Expression<Func<Order, bool>> y = order => order.Customer == customer;
                var e = _sut.Equals(x, y);
                Assert.True(e);
            }

            [Fact]
            public void Equals_Different1_AreNotEqual()
            {
                Expression<Func<Order, object>> x = order => order.Number;
                Expression<Func<Order, object>> y = order => order.LineItems;
                var e = _sut.Equals(x, y);
                Assert.False(e);
            }

            [Fact]
            public void Equals_Different2_AreNotEqual()
            {
                Expression<Func<Order, object>> x = order => order.Customer.Address;
                Expression<Func<Order, object>> y = order => order.LineItems.Select(item => item.Product);
                var e = _sut.Equals(x, y);
                Assert.False(e);
            }

            [Fact]
            public void Equals_Different3_AreNotEqual()
            {
                Expression<Func<Order, object>> x = order => order.Customer.Address;
                Expression<Func<Customer, object>> y = customer => customer.Address;
                var e = _sut.Equals(x, y);
                Assert.False(e);
            }

            [Fact]
            public void Equals_Different4_AreNotEqual()
            {
                Expression<Func<Order, bool>> x = order => order.Number != 5;
                Expression<Func<Order, bool>> y = order => order.Number == 5;
                var e = _sut.Equals(x, y);
                Assert.False(e);
            }

            [Fact]
            public void Equals_Different5_AreNotEqual()
            {
                Expression<Func<Customer, bool>> x = customer => customer.Name == "john";
                Expression<Func<Customer, bool>> y = customer => customer.Name == "paul";
                var e = _sut.Equals(x, y);
                Assert.False(e);
            }

            [Fact]
            public void Equals_Different6_AreNotEqual()
            {
                Expression<Func<Order, bool>> x = order => order.Customer == new Customer { Name = "john" };
                Expression<Func<Order, bool>> y = order => order.Customer == new Customer { Name = "paul" };
                var e = _sut.Equals(x, y);
                Assert.False(e);
            }

            [Fact]
            public void Equals_Different7_AreNotEqual()
            {
                var a = new Customer { Name = "john" };
                var b = new Customer { Name = "paul" };
                Expression<Func<Order, bool>> x = order => order.Customer == a;
                Expression<Func<Order, bool>> y = order => order.Customer == b;
                var e = _sut.Equals(x, y);
                Assert.False(e);
            }

            [Fact]
            public void Equals_Different8_AreNotEqual()
            {
                Expression<Func<Order, bool>> x = order => order.Number < 5;
                Expression<Func<Order, bool>> y = order => order.Number > 5;
                var e = _sut.Equals(x, y);
                Assert.False(e);
            }

            [Fact]
            public void Equals_Different9_AreNotEqual()
            {
                Expression<Func<Order, bool>> x = order => order.Customer.Address.Postcode == 5;
                Expression<Func<Order, bool>> y = order => order.Number > 5;
                var e = _sut.Equals(x, y);
                Assert.False(e);
            }

            [Fact]
            public void GetHashCode_Same1_AreEqual()
            {
                Expression<Func<Order, object>> x = order => order.Customer.Address;
                Expression<Func<Order, object>> y = order => order.Customer.Address;
                Assert.Equal(_sut.GetHashCode(x), _sut.GetHashCode(y));
            }

            [Fact]
            public void GetHashCode_Same2_AreEqual()
            {
                Expression<Func<Order, bool>> x = order => order.Number == 5;
                Expression<Func<Order, bool>> y = order => order.Number == 5;
                Assert.Equal(_sut.GetHashCode(x), _sut.GetHashCode(y));
            }

            [Fact]
            public void GetHashCode_Same3_AreEqual()
            {
                Expression<Func<Order, bool>> x = order => order.Customer == new Customer { Name = "john" };
                Expression<Func<Order, bool>> y = order => order.Customer == new Customer { Name = "john" };
                Assert.Equal(_sut.GetHashCode(x), _sut.GetHashCode(y));
            }

            [Fact]
            public void GetHashCode_Different1_AreNotEqual()
            {
                Expression<Func<Order, object>> x = order => order.Number;
                Expression<Func<Order, object>> y = order => order.LineItems;
                Assert.NotEqual(_sut.GetHashCode(x), _sut.GetHashCode(y));
            }

            [Fact]
            public void GetHashCode_Different2_AreNotEqual()
            {
                Expression<Func<Order, object>> x = order => order.Customer.Address;
                Expression<Func<Order, object>> y = order => order.LineItems.Select(item => item.Product);
                Assert.NotEqual(_sut.GetHashCode(x), _sut.GetHashCode(y));
            }

            [Fact]
            public void GetHashCode_Different3_AreNotEqual()
            {
                Expression<Func<Order, object>> x = order => order.Customer.Address;
                Expression<Func<Customer, object>> y = customer => customer.Address;
                Assert.NotEqual(_sut.GetHashCode(x), _sut.GetHashCode(y));
            }

            [Fact]
            public void GetHashCode_Different4_AreNotEqual()
            {
                Expression<Func<Order, bool>> x = order => order.Number != 5;
                Expression<Func<Order, bool>> y = order => order.Number == 5;
                Assert.NotEqual(_sut.GetHashCode(x), _sut.GetHashCode(y));
            }

            [Fact]
            public void GetHashCode_Different5_AreNotEqual()
            {
                Expression<Func<Customer, bool>> x = customer => customer.Name == "john";
                Expression<Func<Customer, bool>> y = customer => customer.Name == "paul";
                Assert.NotEqual(_sut.GetHashCode(x), _sut.GetHashCode(y));
            }

            [Fact]
            public void GetHashCode_Different6_AreNotEqual()
            {
                Expression<Func<Order, bool>> x = order => order.Customer == new Customer { Name = "john" };
                Expression<Func<Order, bool>> y = order => order.Customer == new Customer { Name = "paul" };
                Assert.NotEqual(_sut.GetHashCode(x), _sut.GetHashCode(y));
            }

            [Fact]
            public void GetHashCode_Different7_AreNotEqual()
            {
                var a = new Customer { Name = "john" };
                var b = new Customer { Name = "paul" };
                Expression<Func<Order, bool>> x = order => order.Customer == a;
                Expression<Func<Order, bool>> y = order => order.Customer == b;
                Assert.NotEqual(_sut.GetHashCode(x), _sut.GetHashCode(y));
            }

            [Fact]
            public void GetHashCode_Different8_AreNotEqual()
            {
                Expression<Func<Order, bool>> x = order => order.Number < 5;
                Expression<Func<Order, bool>> y = order => order.Number > 5;
                Assert.NotEqual(_sut.GetHashCode(x), _sut.GetHashCode(y));
            }

            [Fact]
            public void GetHashCode_Different9_AreNotEqual()
            {
                Expression<Func<Order, bool>> x = order => order.Customer.Address.Postcode == 5;
                Expression<Func<Order, bool>> y = order => order.Number > 5;
                Assert.NotEqual(_sut.GetHashCode(x), _sut.GetHashCode(y));
            }

            [Fact]
            public void BasicConst()
            {
                var const1 = 25;
                var f1 = (Expression<Func<int, string, string>>)((first, second) =>
                   $"{(first + const1).ToString(CultureInfo.InvariantCulture)}{first + second}{"some const value".ToUpper()}{const1}");

                var const2 = "some const value";
                var const3 = "{0}{1}{2}{3}";
                var f2 = (Expression<Func<int, string, string>>)((i, s) =>
                   string.Format(const3, (i + 25).ToString(CultureInfo.InvariantCulture), i + s, const2.ToUpper(), 25));

                Assert.True(_sut.Equals(f1, f2));
            }

            [Fact]
            public void PropAndMethodCall()
            {
                var f1 = (Expression<Func<Uri, bool>>)(arg1 => Uri.IsWellFormedUriString(arg1.ToString(), UriKind.Absolute));
                var f2 = (Expression<Func<Uri, bool>>)(u => Uri.IsWellFormedUriString(u.ToString(), UriKind.Absolute));
                Assert.True(_sut.Equals(f1, f2));
            }

            [Fact]
            public void MemberInitWithConditional()
            {
                var port = 443;
                var f1 = (Expression<Func<Uri, UriBuilder>>)(x => new UriBuilder(x)
                {
                    Port = port,
                    Host = string.IsNullOrEmpty(x.Host) ? "abc" : "def"
                });

                var isSecure = true;
                var f2 = (Expression<Func<Uri, UriBuilder>>)(u => new UriBuilder(u)
                {
                    Host = string.IsNullOrEmpty(u.Host) ? "abc" : "def",
                    Port = isSecure ? 443 : 80
                });
                Assert.True(_sut.Equals(f1, f2));
            }
        }
    }
}