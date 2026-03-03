import Link from "next/link";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";

export default function Home() {
  return (
    <div className="min-h-[calc(100vh-5rem)] relative overflow-hidden">
      <div className="absolute inset-0 bg-gradient-to-b from-background via-background to-background" />
      <div className="absolute top-0 left-0 w-full h-full opacity-5">
        <div className="absolute top-20 left-10 w-96 h-96 bg-[#69CCF0] rounded-full blur-3xl" />
        <div className="absolute bottom-20 right-10 w-96 h-96 bg-[#9482C9] rounded-full blur-3xl" />
        <div className="absolute top-1/2 left-1/2 -translate-x-1/2 -translate-y-1/2 w-96 h-96 bg-[#FF7D0A] rounded-full blur-3xl" />
      </div>
      <div className="container mx-auto px-8 py-24 max-w-7xl relative z-10">
        <div className="text-center mb-24">
          <h1 className="text-6xl md:text-7xl font-bold tracking-tight mb-8 bg-gradient-to-r from-[#69CCF0] via-[#64d2ff] via-[#9482C9] to-[#FF7D0A] bg-clip-text text-transparent leading-tight">
            Nibelung Logs
          </h1>
          <p className="text-xl md:text-2xl text-muted-foreground max-w-2xl mx-auto leading-relaxed font-light">
            Анализ статистики рейдов World of Warcraft. Отслеживайте производительность игроков,
            изучайте детали рейдов и сравнивайте результаты.
          </p>
        </div>

        <div className="grid md:grid-cols-2 gap-6 mb-20">
          <Card className="group relative overflow-hidden border border-border/50 hover:border-[#69CCF0]/40 transition-all duration-500 hover:shadow-2xl hover:shadow-[#69CCF0]/10 hover:-translate-y-2">
            <div className="absolute inset-0 bg-gradient-to-br from-[#69CCF0]/5 via-transparent to-[#ABD473]/5 opacity-0 group-hover:opacity-100 transition-opacity duration-500" />
            <div className="absolute top-0 right-0 w-32 h-32 bg-[#69CCF0]/5 rounded-full blur-2xl opacity-0 group-hover:opacity-100 transition-opacity duration-500" />
            <CardHeader className="relative z-10 pb-4">
              <CardTitle className="text-2xl md:text-3xl font-bold mb-4 bg-gradient-to-r from-[#69CCF0] via-[#ABD473] to-[#FFF569] bg-clip-text text-transparent">
                Игроки
              </CardTitle>
              <CardDescription className="text-base leading-relaxed text-muted-foreground">
                Просматривайте статистику игроков, их DPS, специализации и участие в рейдах
              </CardDescription>
            </CardHeader>
            <CardContent className="relative z-10 pt-2">
              <Link href="/players">
                <Button 
                  variant="default" 
                  className="w-full h-11 text-base font-medium bg-gradient-to-r from-[#69CCF0] to-[#ABD473] hover:from-[#ABD473] hover:to-[#69CCF0] transition-all duration-300 shadow-md hover:shadow-lg hover:shadow-[#69CCF0]/30"
                >
                  Перейти к игрокам
                </Button>
              </Link>
            </CardContent>
          </Card>

          <Card className="group relative overflow-hidden border border-border/50 hover:border-[#C41F3B]/40 transition-all duration-500 hover:shadow-2xl hover:shadow-[#C41F3B]/10 hover:-translate-y-2">
            <div className="absolute inset-0 bg-gradient-to-br from-[#C41F3B]/5 via-transparent to-[#F58CBA]/5 opacity-0 group-hover:opacity-100 transition-opacity duration-500" />
            <div className="absolute top-0 right-0 w-32 h-32 bg-[#C41F3B]/5 rounded-full blur-2xl opacity-0 group-hover:opacity-100 transition-opacity duration-500" />
            <CardHeader className="relative z-10 pb-4">
              <CardTitle className="text-2xl md:text-3xl font-bold mb-4 bg-gradient-to-r from-[#C41F3B] via-[#F58CBA] to-[#FF7D0A] bg-clip-text text-transparent">
                Рейды
              </CardTitle>
              <CardDescription className="text-base leading-relaxed text-muted-foreground">
                Изучайте детальную информацию о рейдах, боссах, вайпах и общей статистике
              </CardDescription>
            </CardHeader>
            <CardContent className="relative z-10 pt-2">
              <Link href="/raids">
                <Button 
                  variant="default" 
                  className="w-full h-11 text-base font-medium bg-gradient-to-r from-[#C41F3B] to-[#F58CBA] hover:from-[#F58CBA] hover:to-[#C41F3B] transition-all duration-300 shadow-md hover:shadow-lg hover:shadow-[#C41F3B]/30"
          >
                  Перейти к рейдам
                </Button>
              </Link>
            </CardContent>
          </Card>
        </div>

        <div className="text-center">
          <p className="text-base text-muted-foreground font-light">
            Выберите раздел выше для начала работы с данными
          </p>
        </div>
      </div>
    </div>
  );
}
