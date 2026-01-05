import Link from 'next/link';

export default function Navigation() {
  return (
    <nav className="bg-[#1a1a1a] border-b border-[#333333] text-[#e5e5e5] sticky top-0 z-50">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div className="flex justify-between items-center h-16">
          <Link href="/" className="text-xl font-bold tracking-tight text-[#e5e5e5] hover:text-white transition-colors">
            Nibelung Log
          </Link>
          <div className="flex gap-6">
            <Link href="/raids" className="text-sm font-medium text-[#9ca3af] hover:text-[#e5e5e5] transition-colors">
              Рейды
            </Link>
            <Link href="/players" className="text-sm font-medium text-[#e5e5e5] hover:text-white transition-colors">
              Игроки
            </Link>
          </div>
        </div>
      </div>
    </nav>
  );
}

