import Link from 'next/link';

export default function Navigation() {
  return (
    <nav 
      className="sticky top-0 z-50 bg-[var(--background-elevated)] border-b border-[var(--border-color)] backdrop-blur-md bg-opacity-80"
      role="navigation"
      aria-label="Основная навигация"
    >
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div className="flex justify-between items-center h-16">
          <Link 
            href="/" 
            className="text-xl font-bold tracking-tight text-[var(--foreground)] hover:text-[var(--primary)] transition-colors focus:outline-none focus:ring-2 focus:ring-[var(--accent)] focus:ring-offset-2 focus:ring-offset-[var(--background-elevated)] rounded-md px-2 py-1"
            aria-label="Главная страница"
          >
            Nibelung Log
          </Link>
          <div className="flex gap-8" role="menubar">
            <Link 
              href="/raids" 
              className="text-sm font-medium text-[var(--foreground-muted)] hover:text-[var(--foreground)] transition-colors focus:outline-none focus:ring-2 focus:ring-[var(--accent)] focus:ring-offset-2 focus:ring-offset-[var(--background-elevated)] rounded-md px-3 py-2"
              role="menuitem"
            >
              Рейды
            </Link>
            <Link 
              href="/players" 
              className="text-sm font-medium text-[var(--foreground)] hover:text-[var(--primary)] transition-colors focus:outline-none focus:ring-2 focus:ring-[var(--accent)] focus:ring-offset-2 focus:ring-offset-[var(--background-elevated)] rounded-md px-3 py-2"
              role="menuitem"
            >
              Игроки
            </Link>
          </div>
        </div>
      </div>
    </nav>
  );
}

