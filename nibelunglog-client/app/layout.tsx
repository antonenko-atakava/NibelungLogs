import type { Metadata } from "next";
import { Geist, Geist_Mono } from "next/font/google";
import "./globals.css";
import Navigation from "@/components/sections/navigation/Navigation";

const geistSans = Geist({
  variable: "--font-geist-sans",
  subsets: ["latin"],
});

const geistMono = Geist_Mono({
  variable: "--font-geist-mono",
  subsets: ["latin"],
});

export const metadata: Metadata = {
  title: "Nibelung Log - Логи рейдов WoW",
  description: "Анализ производительности игроков, статистика рейдов и детальная информация о каждом энкаунтере",
};

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html lang="ru">
            <body
              className={`${geistSans.variable} ${geistMono.variable} antialiased bg-[#1a1a1a] text-[#e5e5e5]`}
            >
        <Navigation />
        {children}
      </body>
    </html>
  );
}
