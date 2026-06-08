import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { MemoryRouter } from 'react-router-dom'
import { describe, it, expect, vi, beforeEach } from 'vitest'
import { LoginPage } from './LoginPage'
import { AuthContext } from '../context/AuthContext'

const mockNavigate = vi.fn()
vi.mock('react-router-dom', async (importOriginal) => {
  const mod = await importOriginal<typeof import('react-router-dom')>()
  return { ...mod, useNavigate: () => mockNavigate }
})

function makeAuthContext(overrides: Partial<{ login: () => Promise<void> }> = {}) {
  return {
    token: null,
    register: vi.fn(),
    login: vi.fn().mockResolvedValue(undefined),
    logout: vi.fn(),
    ...overrides,
  }
}

function renderLogin(ctx = makeAuthContext()) {
  return render(
    <MemoryRouter>
      <AuthContext.Provider value={ctx}>
        <LoginPage />
      </AuthContext.Provider>
    </MemoryRouter>,
  )
}

describe('LoginPage', () => {
  beforeEach(() => {
    mockNavigate.mockReset()
  })

  it('renders email and password fields', () => {
    renderLogin()
    expect(screen.getByPlaceholderText('korisnik@example.com')).toBeInTheDocument()
    expect(screen.getByPlaceholderText('••••••••')).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Prijavi se' })).toBeInTheDocument()
  })

  it('calls login and navigates to /dashboard on success', async () => {
    const login = vi.fn().mockResolvedValue(undefined)
    renderLogin(makeAuthContext({ login }))

    await userEvent.type(screen.getByPlaceholderText('korisnik@example.com'), 'user@example.com')
    await userEvent.type(screen.getByPlaceholderText('••••••••'), 'secret123')
    await userEvent.click(screen.getByRole('button', { name: 'Prijavi se' }))

    await waitFor(() => expect(login).toHaveBeenCalledWith('user@example.com', 'secret123'))
    expect(mockNavigate).toHaveBeenCalledWith('/dashboard', { replace: true })
  })

  it('shows error message on failed login', async () => {
    const login = vi.fn().mockRejectedValue({ response: { status: 401 } })
    renderLogin(makeAuthContext({ login }))

    await userEvent.type(screen.getByPlaceholderText('korisnik@example.com'), 'bad@example.com')
    await userEvent.type(screen.getByPlaceholderText('••••••••'), 'wrong')
    await userEvent.click(screen.getByRole('button', { name: 'Prijavi se' }))

    await waitFor(() =>
      expect(screen.getByText('Pogrešan email ili lozinka.')).toBeInTheDocument(),
    )
    expect(mockNavigate).not.toHaveBeenCalled()
  })
})
