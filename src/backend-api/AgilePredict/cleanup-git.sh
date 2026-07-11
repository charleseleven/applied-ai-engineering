# Script para Limpar Arquivos .vs do Git
# Execute este script no Git Bash ou terminal com Git instalado

echo "=== Limpando arquivos .vs do Git ==="

# Remove arquivos .vs do cache do Git (não deleta localmente)
git rm -r --cached .vs/ 2>/dev/null
git rm -r --cached src/backend-api/AgilePredict/.vs/ 2>/dev/null
git rm -r --cached AgilePredict/.vs/ 2>/dev/null

# Remove arquivos bin e obj também
git rm -r --cached **/bin/ 2>/dev/null
git rm -r --cached **/obj/ 2>/dev/null

# Remove arquivos temporários de instrução
git rm --cached INSTRUCOES_*.md 2>/dev/null
git rm --cached SOLUCAO_*.md 2>/dev/null

echo "=== Limpeza concluída ==="
echo ""
echo "Agora execute:"
echo "git add ."
echo "git commit -m \"chore: Adiciona .gitignore e remove arquivos do Visual Studio\""
echo "git push"
