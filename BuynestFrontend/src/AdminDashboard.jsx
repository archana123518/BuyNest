
import { useEffect, useMemo, useState } from 'react'
import './AdminDashboard.css'

const API = 'http://localhost:5139/api'

function AdminDashboard({ onBack }) {
  const [products, setProducts] = useState([])
  const [orders, setOrders] = useState([])
  const [activeTab, setActiveTab] = useState('Overview')
  const [search, setSearch] = useState('')
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')

  const getToken = () => localStorage.getItem('token')

  const loadDashboard = async () => {
    setLoading(true)
    setError('')

    try {
      const headers = {
        Authorization: `Bearer ${getToken()}`
      }

      const [productResponse, orderResponse] = await Promise.all([
        fetch(`${API}/Products`, { headers }),
        fetch(`${API}/Orders/admin`, { headers })
      ])

      if (!productResponse.ok) {
        throw new Error('Unable to load products. Please check your admin login.')
      }

      const productData = await productResponse.json()
      const orderData = orderResponse.ok ? await orderResponse.json() : []

      setProducts(Array.isArray(productData) ? productData : [])
      setOrders(Array.isArray(orderData) ? orderData : [])
    } catch (err) {
      setError(err.message || 'Something went wrong.')
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    loadDashboard()
  }, [])

  const totalRevenue = orders.reduce(
    (total, order) =>
      total + Number(order.totalAmount ?? order.TotalAmount ?? 0),
    0
  )

  const lowStock = products.filter(
    product => Number(product.stockQuantity ?? product.StockQuantity ?? 0) <= 5
  ).length

  const filteredProducts = useMemo(() => {
    const term = search.toLowerCase()

    return products.filter(product =>
      String(product.name ?? product.Name ?? '')
        .toLowerCase()
        .includes(term)
    )
  }, [products, search])

  const money = amount =>
    new Intl.NumberFormat('en-IN', {
      style: 'currency',
      currency: 'INR',
      maximumFractionDigits: 2
    }).format(amount)

  const menuItems = [
    { name: 'Overview', icon: '▦' },
    { name: 'Products', icon: '□' },
    { name: 'Orders', icon: '▤' }
  ]

  return (
    <div className="admin-layout">
      <aside className="admin-sidebar">
        <div className="admin-brand">
          <span className="admin-brand-icon">B</span>
          <span>buynest<span className="brand-dot">.</span></span>
        </div>

        <p className="sidebar-label">WORKSPACE</p>

        <nav className="admin-menu">
          {menuItems.map(item => (
            <button
              key={item.name}
              className={activeTab === item.name ? 'active' : ''}
              onClick={() => setActiveTab(item.name)}
            >
              <span className="menu-icon">{item.icon}</span>
              {item.name}
            </button>
          ))}
        </nav>

        <div className="sidebar-bottom">
          <div className="admin-profile">
            <div className="profile-avatar">A</div>
            <div>
              <strong>Administrator</strong>
              <small>Store Manager</small>
            </div>
          </div>

          <button className="back-store-btn" onClick={onBack}>
            ← Back to Store
          </button>
        </div>
      </aside>

      <main className="admin-main">
        <header className="admin-topbar">
          <div>
            <p className="admin-breadcrumb">BUYNEST / ADMIN</p>
            <h1>{activeTab}</h1>
          </div>

          <div className="admin-top-actions">
            <span className="admin-status">
              <span /> Store Active
            </span>
            <button
              className="refresh-btn"
              onClick={loadDashboard}
              title="Refresh dashboard"
            >
              ↻
            </button>
          </div>
        </header>

        {loading ? (
          <div className="admin-message">Loading your store dashboard...</div>
        ) : error ? (
          <div className="admin-error">
            <p>{error}</p>
            <button onClick={loadDashboard}>Try Again</button>
          </div>
        ) : (
          <>
            {activeTab === 'Overview' && (
              <>
                <section className="admin-welcome">
                  <div>
                    <span className="welcome-tag">STORE OVERVIEW</span>
                    <h2>Your store, at a glance.</h2>
                    <p>Track products, orders and your store performance.</p>
                  </div>
                  <div className="welcome-symbol">B.</div>
                </section>

                <section className="admin-stats">
                  <div className="stat-card">
                    <span className="stat-icon orange">▣</span>
                    <p>Total Products</p>
                    <h2>{products.length}</h2>
                    <small>Products in inventory</small>
                  </div>

                  <div className="stat-card">
                    <span className="stat-icon purple">▤</span>
                    <p>Total Orders</p>
                    <h2>{orders.length}</h2>
                    <small>Orders received</small>
                  </div>

                  <div className="stat-card">
                    <span className="stat-icon green">₹</span>
                    <p>Order Value</p>
                    <h2>{money(totalRevenue)}</h2>
                    <small>Value of loaded orders</small>
                  </div>

                  <div className="stat-card">
                    <span className="stat-icon red">!</span>
                    <p>Low Stock</p>
                    <h2>{lowStock}</h2>
                    <small>Products with 5 or fewer items</small>
                  </div>
                </section>

                <section className="admin-panel">
                  <div className="panel-heading">
                    <div>
                      <h3>Recent Products</h3>
                      <p>Your current product inventory</p>
                    </div>
                    <button onClick={() => setActiveTab('Products')}>
                      View All →
                    </button>
                  </div>

                  <ProductTable products={products.slice(0, 5)} money={money} />
                </section>

                <section className="admin-panel">
                  <div className="panel-heading">
                    <div>
                      <h3>Recent Orders</h3>
                      <p>Latest orders from your store</p>
                    </div>
                    <button onClick={() => setActiveTab('Orders')}>
                      View All →
                    </button>
                  </div>

                  <OrderTable orders={orders.slice(0, 5)} money={money} />
                </section>
              </>
            )}

            {activeTab === 'Products' && (
              <section className="admin-panel full-panel">
                <div className="panel-heading">
                  <div>
                    <h3>Product Inventory</h3>
                    <p>{products.length} products in your store</p>
                  </div>
                  <input
                    className="admin-search"
                    placeholder="Search products..."
                    value={search}
                    onChange={e => setSearch(e.target.value)}
                  />
                </div>

                <ProductTable products={filteredProducts} money={money} />
              </section>
            )}

            {activeTab === 'Orders' && (
              <section className="admin-panel full-panel">
                <div className="panel-heading">
                  <div>
                    <h3>All Orders</h3>
                    <p>{orders.length} orders loaded</p>
                  </div>
                </div>
                <OrderTable orders={orders} money={money} />
              </section>
            )}
          </>
        )}
      </main>
    </div>
  )
}

function ProductTable({ products, money }) {
  const [discountValues, setDiscountValues] = useState({})
  const [savingId, setSavingId] = useState(null)
  const [savedDiscounts, setSavedDiscounts] = useState({})

  const saveDiscount = async product => {
    const id = product.id ?? product.Id
    const key = String(id)

    const currentDiscount = Number(
      product.discountPercentage ?? product.DiscountPercentage ?? 0
    )

    const discount = Number(
      discountValues[key] ?? savedDiscounts[key] ?? currentDiscount
    )

    if (!Number.isFinite(discount) || discount < 0 || discount > 90) {
      alert('Discount must be between 0 and 90.')
      return
    }

    const price = Number(product.price ?? product.Price ?? 0)

    const requestBody = {
      id,
      name: product.name ?? product.Name ?? '',
      description: product.description ?? product.Description ?? '',
      price,
      discountPercentage: discount,
      stockQuantity: Number(
        product.stockQuantity ?? product.StockQuantity ?? 0
      ),
      imageUrl: product.imageUrl ?? product.ImageUrl ?? '',
      categoryId:
        product.categoryId ??
        product.CategoryId ??
        product.category?.id ??
        product.Category?.Id
    }

    if (requestBody.categoryId == null) {
      alert('Category ID is missing for this product.')
      return
    }

    try {
      setSavingId(key)

      const response = await fetch(`${API}/Products/${id}`, {
        method: 'PUT',
        headers: {
          'Content-Type': 'application/json',
          Authorization: `Bearer ${localStorage.getItem('token')}`
        },
        body: JSON.stringify(requestBody)
      })

      if (!response.ok) {
        const message = await response.text()
        throw new Error(message || 'Discount update failed.')
      }

      setSavedDiscounts(prev => ({
        ...prev,
        [key]: discount
      }))

      alert(`${requestBody.name}: ${discount}% discount saved successfully!`)
    } catch (error) {
      console.error('Discount update error:', error)
      alert(error.message || 'Unable to update discount.')
    } finally {
      setSavingId(null)
    }
  }

  if (!products.length) {
    return <p className="admin-empty">No products found.</p>
  }

  return (
    <div className="admin-table-wrap">
      <table className="admin-table">
        <thead>
          <tr>
            <th>PRODUCT</th>
            <th>ORIGINAL PRICE</th>
            <th>SET DISCOUNT</th>
            <th>FINAL PRICE</th>
            <th>STOCK</th>
            <th>STATUS</th>
          </tr>
        </thead>

        <tbody>
          {products.map((product, index) => {
            const id = product.id ?? product.Id
            const key = String(id)
            const name = product.name ?? product.Name ?? 'Product'

            const stock = Number(
              product.stockQuantity ?? product.StockQuantity ?? 0
            )

            const image = product.imageUrl ?? product.ImageUrl

            const price = Number(product.price ?? product.Price ?? 0)

            const originalDiscount = Number(
              product.discountPercentage ?? product.DiscountPercentage ?? 0
            )

            const discount = Number(
              discountValues[key] ??
              savedDiscounts[key] ??
              originalDiscount
            )

            const discountedPrice =
              price - (price * discount / 100)

            return (
              <tr key={id ?? index}>
                <td>
                  <div className="product-cell">
                    {image ? (
                      <img src={image} alt={name} />
                    ) : (
                      <div className="product-placeholder">B</div>
                    )}
                    <strong>{name}</strong>
                  </div>
                </td>

                <td>{money(price)}</td>

                <td>
                  <div style={{ display: 'flex', alignItems: 'center', gap: '6px' }}>
                    <input
                      type="number"
                      min="0"
                      max="90"
                      step="1"
                      value={
                        discountValues[key] ??
                        savedDiscounts[key] ??
                        originalDiscount
                      }
                      onChange={e =>
                        setDiscountValues(prev => ({
                          ...prev,
                          [key]: e.target.value
                        }))
                      }
                      style={{
                        width: '65px',
                        padding: '7px',
                        border: '1px solid #ddd',
                        borderRadius: '6px'
                      }}
                    />
                    <span>%</span>
                    <button
                      type="button"
                      onClick={() => saveDiscount(product)}
                      disabled={savingId === key}
                      style={{
                        padding: '7px 10px',
                        border: 'none',
                        borderRadius: '6px',
                        background: '#f97316',
                        color: '#fff',
                        cursor: savingId === key ? 'not-allowed' : 'pointer',
                        opacity: savingId === key ? 0.6 : 1
                      }}
                    >
                      {savingId === key ? 'Saving...' : 'Save'}
                    </button>
                  </div>
                </td>

                <td>
                  <strong>{money(discountedPrice)}</strong>
                  {discount > 0 && (
                    <small style={{ display: 'block', color: '#e63946' }}>
                      {discount}% OFF
                    </small>
                  )}
                </td>

                <td>{stock}</td>

                <td>
                  <span className={`stock-badge ${stock <= 5 ? 'low' : 'available'}`}>
                    {stock <= 5 ? 'Low Stock' : 'Available'}
                  </span>
                </td>
              </tr>
            )
          })}
        </tbody>
      </table>
    </div>
  )
}

function OrderTable({ orders, money }) {
  const statuses = [
    'Pending',
    'Ordered',
    'Packed',
    'Shipped',
    'Out for Delivery',
    'Delivered'
  ]

  const [deliveryDates, setDeliveryDates] = useState({})

  const handleStatusChange = async (orderId, newStatus) => {
    try {
      const token = localStorage.getItem('token')

      const response = await fetch(
        `${API}/Orders/${orderId}/status`,
        {
          method: 'PUT',
          headers: {
            'Content-Type': 'application/json',
            Authorization: `Bearer ${token}`
          },
          body: JSON.stringify(newStatus)
        }
      )

      if (!response.ok) {
        const message = await response.text()
        alert(message || 'Status update failed')
        return
      }

      alert('Order status updated successfully!')
      window.location.reload()
    } catch (error) {
      console.error(error)
      alert('Unable to update order status')
    }
  }

  const handleDeliveryDate = async (orderId, currentDate) => {
    try {
      const token = localStorage.getItem('token')

      const response = await fetch(
        `${API}/Orders/${orderId}/delivery-date`,
        {
          method: 'PUT',
          headers: {
            'Content-Type': 'application/json',
            Authorization: `Bearer ${token}`
          },
          body: JSON.stringify(
            currentDate ? new Date(currentDate + 'T00:00:00').toISOString() : null
          )
        }
      )

      if (!response.ok) {
        const message = await response.text()
        alert(message || 'Delivery date update failed')
        return
      }

      alert('Delivery date saved successfully!')
      window.location.reload()
    } catch (error) {
      console.error(error)
      alert('Unable to save delivery date')
    }
  }

  if (!orders.length) {
    return <p className="admin-empty">No orders found.</p>
  }

  return (
    <div className="admin-table-wrap">
      <table className="admin-table">
        <thead>
          <tr>
            <th>ORDER ID</th>
            <th>DATE</th>
            <th>ITEMS</th>
            <th>AMOUNT</th>
            <th>STATUS</th>
            <th>EXPECTED DELIVERY</th>
          </tr>
        </thead>

        <tbody>
          {orders.map((order, index) => {
            const id = order.id ?? order.Id ?? index
            const date = order.orderDate ?? order.OrderDate
            const items = order.orderItems ?? order.OrderItems ?? []
            const status = order.status ?? order.Status ?? 'Pending'

            const savedDate =
              order.expectedDeliveryDate ??
              order.ExpectedDeliveryDate

            const selectedDate =
              deliveryDates[id] ??
              (savedDate ? String(savedDate).slice(0, 10) : '')

            return (
              <tr key={id}>
                <td><strong>#{id}</strong></td>

                <td>
                  {date ? new Date(date).toLocaleDateString('en-IN') : '—'}
                </td>

                <td>{Array.isArray(items) ? items.length : 0}</td>

                <td>
                  {money(Number(order.totalAmount ?? order.TotalAmount ?? 0))}
                </td>

                <td>
                  <select
                    value={status}
                    onChange={e => handleStatusChange(id, e.target.value)}
                  >
                    {statuses.map(item => (
                      <option key={item} value={item}>
                        {item}
                      </option>
                    ))}
                  </select>
                </td>

                <td>
                  <input
                    type="date"
                    value={selectedDate}
                    onChange={e =>
                      setDeliveryDates(prev => ({
                        ...prev,
                        [id]: e.target.value
                      }))
                    }
                  />

                  <button
                    type="button"
                    onClick={() => handleDeliveryDate(id, selectedDate)}
                  >
                    Save
                  </button>
                </td>
              </tr>
            )
          })}
        </tbody>
      </table>
    </div>
  )
}

export default AdminDashboard
