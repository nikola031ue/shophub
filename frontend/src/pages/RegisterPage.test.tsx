import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { MemoryRouter } from 'react-router-dom'
import { describe, it, expect, vi, beforeEach } from 'vitest'
import { RegisterPage } from './RegisterPage'
import { AuthContext } from '../context/AuthContext'

const mockNavigate = vi.fn()
vi.mock('react-router-dom', async (importOriginal) => {
  const mod = await importOriginal<typeof import('react-router-dom')>()
  return { ...mod, useNavigate: () => mockNavigate }
})

function makeAuthContext(overrides: Partial<{ register: () => Promise<void> }> = {}) {
  return {
    token: null,
    register: vi.fn().mockResolvedValue(undefined),
    login: vi.fn(),
    logout: vi.fn(),
    ...overrides,
  }
}

function renderRegister(ctx = makeAuthContext()) {
  return render(
    <MemoryRouter>
      <AuthContext.Provider value={ctx}>
        <RegisterPage />
      </AuthContext.Provider>
    </MemoryRouter>,
  )
}

describe('RegisterPage', () => {
  beforeEach(() => {
    mockNavigate.mockReset()
  })

  it('renders all form fields', () => {
    renderRegister()
    expect(screen.getByPlaceholderText('korisnik@example.com')).toBeInTheDocument()
    expect(screen.getAllByPlaceholderText('••••••••')).toHaveLength(2)
    expect(screen.getByRole('button', { name: 'Registruj se' })).toBeInTheDocument()
  })

  it('calls register and navigates to /dashboard on success', async () => {
    const register = vi.fn().mockResolvedValue(undefined)
    renderRegister(makeAuthContext({ register }))

    await userEvent.type(screen.getByPlaceholderText('korisnik@example.com'), 'new@example.com')
    const [pass, confirm] = screen.getAllByPlaceholderText('••••••••')
    await userEvent.type(pass, 'password123')
    await userEvent.type(confirm, 'password123')
    await userEvent.click(screen.getByRole('button', { name: 'Registruj se' }))

    await waitFor(() => expect(register).toHaveBeenCalledWith('new@example.com', 'password123'))
    expect(mockNavigate).toHaveBeenCalledWith('/dashboard', { replace: true })
  })

  it('shows error when passwords do not match', async () => {
    renderRegister()

    await userEvent.type(screen.getByPlaceholderText('korisnik@example.com'), 'new@example.com')
    const [pass, confirm] = screen.getAllByPlaceholderText('••••••••')
    await userEvent.type(pass, 'password123')
    await userEvent.type(confirm, 'different')
    await userEvent.click(screen.getByRole('button', { name: 'Registruj se' }))

    expect(screen.getByText('Lozinke se ne poklapaju.')).toBeInTheDocument()
    expect(mockNavigate).not.toHaveBeenCalled()
  })

  it('shows 409 error when email already exists', async () => {
    const register = vi.fn().mockRejectedValue({ response: { status: 409 } })
    renderRegister(makeAuthContext({ register }))

    await userEvent.type(screen.getByPlaceholderText('korisnik@example.com'), 'taken@example.com')
    const [pass, confirm] = screen.getAllByPlaceholderText('••••••••')
    await userEvent.type(pass, 'password123')
    await userEvent.type(confirm, 'password123')
    await userEvent.click(screen.getByRole('button', { name: 'Registruj se' }))

    await waitFor(() =>
      expect(screen.getByText('Email je već registrovan.')).toBeInTheDocument(),
    )
    expect(mockNavigate).not.toHaveBeenCalled()
  })
})
