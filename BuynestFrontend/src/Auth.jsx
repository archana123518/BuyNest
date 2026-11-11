
import { useState } from 'react'
import './Auth.css'

function Auth({ onLogin, initialIsRegister = false }) {
  const [isRegister, setIsRegister] = useState(initialIsRegister)
  const [fullName, setFullName] = useState('')
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [showPassword, setShowPassword] = useState(false)
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState('')
  const [message, setMessage] = useState('')

  const handleSubmit = async (e) => {
    e.preventDefault()
    setError('')
    setMessage('')
    setLoading(true)

    try {
      const endpoint = isRegister ? 'register' : 'login'

      const body = isRegister
        ? { fullName, email, password }
        : { email, password }

      const response = await fetch(
        `http://localhost:5139/api/Auth/${endpoint}`,
        {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify(body)
        }
      )

      const data = await response.json().catch(() => null)

      if (!response.ok) {
        throw new Error(
          typeof data === 'string'
            ? data
            : data?.[0]?.description ||
              data?.message ||
              'Something went wrong. Please try again.'
        )
      }

      if (isRegister) {
        setMessage('Account created successfully! Please sign in.')
        setIsRegister(false)
        setPassword('')
        setShowPassword(false)
      } else {
        const receivedToken = data?.token || data?.accessToken

if (!receivedToken) {
  throw new Error('Login token not received from server.')
}

localStorage.setItem('token', receivedToken)
onLogin(receivedToken)
      }
    } catch (err) {
      setError(err.message)
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="auth-page">
      <div className="auth-card">

        <h1 className="auth-logo">
          Buy<span>nest</span>
        </h1>

        <h2>
          {isRegister ? 'Create Your Account' : 'Welcome Back'}
        </h2>

        <p className="auth-subtitle">
          {isRegister
            ? 'Join Buynest and start exploring.'
            : 'Sign in to continue shopping.'}
        </p>

        {error && <p className="auth-error">{error}</p>}
        {message && <p className="auth-success">{message}</p>}

        <form onSubmit={handleSubmit}>

          {isRegister && (
            <input
              type="text"
              placeholder="Full Name"
              value={fullName}
              onChange={e => setFullName(e.target.value)}
              required
            />
          )}

          <input
            type="email"
            placeholder="Email Address"
            value={email}
            onChange={e => setEmail(e.target.value)}
            required
          />

          <div className="password-field">
            <input
              type={showPassword ? 'text' : 'password'}
              placeholder="Password"
              value={password}
              onChange={e => setPassword(e.target.value)}
              minLength={isRegister ? 8 : undefined}
              required
            />

            <button
              type="button"
              className="password-toggle"
              onClick={() => setShowPassword(!showPassword)}
            >
              {showPassword ? '🙈' : '👁️'}
            </button>
          </div>

          <button type="submit" disabled={loading}>
            {loading
              ? 'Please wait...'
              : isRegister
                ? 'Create Account'
                : 'Sign In'}
          </button>

        </form>

        <p className="auth-switch">
          {isRegister
            ? 'Already have an account?'
            : "Don't have an account?"}

          <button
            type="button"
            onClick={() => {
              setIsRegister(!isRegister)
              setError('')
              setMessage('')
              setPassword('')
              setShowPassword(false)
            }}
          >
            {isRegister ? 'Sign In' : 'Register'}
          </button>
        </p>

      </div>
    </div>
  )
}

export default Auth
