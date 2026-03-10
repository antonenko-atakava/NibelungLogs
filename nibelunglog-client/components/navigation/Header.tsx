"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import { cn } from "@/lib/utils";

const navigationItems = [
  { href: "/", label: "Главная" },
  { href: "/players", label: "Игроки" },
  { href: "/guilds", label: "Гильдии" },
  { href: "/raids", label: "Рейды" },
];

export function Header() {
  const pathname = usePathname();

  return (
    <header className="sticky top-0 z-50 w-full border-b border-border/30 bg-background/70 backdrop-blur-xl supports-[backdrop-filter]:bg-background/50">
      <div className="container mx-auto flex h-20 items-center justify-between px-8 max-w-7xl">
        <Link href="/" className="flex items-center space-x-3 group">
          <span className="text-2xl font-bold text-foreground group-hover:text-primary transition-colors">
            Nibelung Logs
          </span>
        </Link>
        <nav className="flex items-center space-x-1">
          {navigationItems.map((item) => {
            const isActive = pathname === item.href;
            return (
              <Link
                key={item.href}
                href={item.href}
                className={cn(
                  "px-6 py-3 text-sm font-medium transition-all rounded-lg relative",
                  isActive
                    ? "text-primary bg-primary/10"
                    : "text-muted-foreground hover:text-foreground hover:bg-secondary/40"
                )}
              >
                {item.label}
                {isActive && (
                  <span className="absolute bottom-1 left-1/2 -translate-x-1/2 w-1 h-1 rounded-full bg-primary" />
                )}
              </Link>
            );
          })}
        </nav>
      </div>
    </header>
  );
}
